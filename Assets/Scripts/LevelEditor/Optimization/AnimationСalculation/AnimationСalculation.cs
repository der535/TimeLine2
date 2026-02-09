using System;
using System.Collections.Generic;
using System.Diagnostics;
using EventBus;
using TimeLine.EventBus.Events.Misc;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.Keyframe;
using TimeLine.Keyframe.AnimationDatas.TransformComponent;
using TimeLine.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.Keyframe.AnimationDatas.TransformComponent.Rotation;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Scale;
using TimeLine.TimeLine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using Zenject;

namespace TimeLine.LevelEditor.Optimization
{
    /// <summary>
    /// Переводит тяжелые вычисления из основного потока в фоновые воркеры.
    /// </summary>
    public class AnimationСalculation : MonoBehaviour
    {
        // --- Зависимости ---
        private GameEventBus _gameEventBus;
        private KeyframeTrackStorage _trackStorage;
        private PlayAndStopButton _playAndStopButton;

        [Inject]
        private void Construct(GameEventBus gameEventBus, KeyframeTrackStorage trackStorage,
            PlayAndStopButton playModeController)
        {
            _gameEventBus = gameEventBus;
            _trackStorage = trackStorage;
            _playAndStopButton = playModeController;
        }

        // --- Native Контейнеры (Blittable данные для Jobs) ---
        private NativeArray<JobKeyframe> _allKeyframes; // Все ключи всех треков в одном плоском массиве
        private NativeArray<JobTrackInfo> _trackInfos; // Данные о границах каждого трека в _allKeyframes
        private NativeArray<float4> _results; // Рассчитанные значения для текущего кадра
        private NativeArray<int> _resultIndices; // Соответствие индекса транформа индексу результата
        private NativeArray<TransformMask> _transformMasks; // Что именно двигаем (Pos, Rot, Scale)
        private TransformAccessArray _transformAccessArray; // Специальный массив для прямой работы с Transform из Jobs

        // --- Обычные списки для синхронных операций ---
        private List<(IAnimationApplyer applyer, Component target, int resIdx)> _manualApplyers = new();
        private List<Component> _notifyTargets = new();

        private void Start()
        {
            // Подписка на событие тика времени
            _gameEventBus.SubscribeTo<TickSmoothTimeEvent>(UpdateTime);

            // Автоматический "запекание" данных при старте проигрывания
            _gameEventBus.SubscribeTo((ref ChangePlayMode _) =>
            {
                if (_.IsPlaying) Bake();
            });
        }

        private void OnDestroy()
        {
            // Обязательная очистка памяти при удалении/выключении
            if (_allKeyframes.IsCreated) _allKeyframes.Dispose();
            if (_trackInfos.IsCreated) _trackInfos.Dispose();
            if (_results.IsCreated) _results.Dispose();
            if (_transformMasks.IsCreated) _transformMasks.Dispose();
            if (_resultIndices.IsCreated) _resultIndices.Dispose();
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
        }

        /// <summary>
        /// Основной цикл обновления анимаций
        /// </summary>
        public void UpdateTime(ref TickSmoothTimeEvent tick)
        {
            if (!_playAndStopButton._isPlaying) return;
            if (!_trackInfos.IsCreated || _trackInfos.Length == 0) return;

            Profiler.BeginSample("MySystem.UpdateTime");

            // 1. Подготовка и запуск AnimationJob (Математика интерполяции)
            var animJob = new AnimationJob
            {
                AllKeyframes = _allKeyframes,
                TrackInfos = _trackInfos,
                Results = _results,
                CurrentTime = tick.Time,
                SecondsPerTick = (float)TimeLineConverter.Instance.SecondsPerTick
            };
            JobHandle animHandle = animJob.Schedule(_trackInfos.Length, 64);

            // 2. Подготовка и запуск ApplyTransformJob (Применение к объектам в сцене)
            JobHandle finalHandle = animHandle;
            if (_transformAccessArray.isCreated && _transformAccessArray.length > 0)
            {
                var transJob = new ApplyTransformJob
                {
                    Results = _results,
                    ResultIndices = _resultIndices,
                    Masks = _transformMasks
                };
                // transJob начнет работу только ПОСЛЕ завершения animHandle
                finalHandle = transJob.Schedule(_transformAccessArray, animHandle);
            }

            // 3. Синхронизация: ждем завершения всех потоков
            finalHandle.Complete();

            Profiler.EndSample();

            // 4. Оповещение кастомных компонентов
            TransformComponentStorage.InvokeAllComponents();

            // 5. Ручное применение для типов, которые не поддерживаются в Job System (напр. UI или специфичные шейдеры)
            for (int i = 0; i < _manualApplyers.Count; i++)
            {
                var item = _manualApplyers[i];
                item.applyer.Apply(item.target, _results[item.resIdx]);
            }
        }

        /// <summary>
        /// Конвертация данных из объектной модели Unity в Native-массивы
        /// </summary>
        private void Bake()
        {
            var sourceTracks = _trackStorage.GetTracks();


            OnDestroy(); // Очищаем старые данные
            _manualApplyers.Clear();
            _notifyTargets.Clear();


            int totalKeysCount = 0;
            foreach (var track in sourceTracks) totalKeysCount += track.Track.Keyframes.Count;

            // Инициализация массивов
            _allKeyframes = new NativeArray<JobKeyframe>(totalKeysCount, Allocator.Persistent);
            _trackInfos = new NativeArray<JobTrackInfo>(sourceTracks.Count, Allocator.Persistent);
            _results = new NativeArray<float4>(sourceTracks.Count, Allocator.Persistent);

            List<Transform> transformsForJob = new();
            List<int> resultIndicesForJob = new();
            List<TransformMask> masksForJob = new();

            int currentKeyIndex = 0;
            for (int i = 0; i < sourceTracks.Count; i++)
            {
                var src = sourceTracks[i];

                // Заполнение паспорта трека
                _trackInfos[i] = new JobTrackInfo
                {
                    StartKeyIndex = currentKeyIndex,
                    KeyCount = src.Track.Keyframes.Count,
                    Offset = src.Track.GetOffset() + src.TrackObject.StartTimeInTicks,
                    ResultIndex = i
                };

                // Копирование ключей
                for (int j = 0; j < src.Track.Keyframes.Count; j++)
                {
                    var kf = src.Track.Keyframes[j];
                    _allKeyframes[currentKeyIndex] = new JobKeyframe
                    {
                        Ticks = kf.Ticks,
                        Value = kf.GetData().PackDataToFloat4(),
                        Interpolation = (int)kf.Interpolation,
                        OutTangent = new float4((float)kf.OutTangent),
                        InTangent = new float4((float)kf.InTangent),
                        OutWeight = (float)kf.OutWeight,
                        InWeight = (float)kf.InWeight
                    };
                    currentKeyIndex++;
                }

                // Распределение по маскам (Pos, Rot, Scale)
                var animData = src.Track.GetAnimationData();
                TransformMask mask = GetMaskFromData(animData);

                if (mask != TransformMask.None && src.Track.cachedComponent != null)
                {
                    transformsForJob.Add(src.Track.cachedComponent.transform);
                    resultIndicesForJob.Add(i);
                    masksForJob.Add(mask);
                    _notifyTargets.Add(src.Track.cachedComponent);
                }
                else if (src.Track.cachedComponent != null) // Добавьте эту проверку
                {
                    _manualApplyers.Add((src.Track.GetApplyer().Item1, src.Track.cachedComponent, i));
                }
            }

            // Создание специализированных массивов для Job
            _transformAccessArray = new TransformAccessArray(transformsForJob.ToArray());
            _resultIndices = new NativeArray<int>(resultIndicesForJob.ToArray(), Allocator.Persistent);
            _transformMasks = new NativeArray<TransformMask>(masksForJob.ToArray(), Allocator.Persistent);
        }

        private TransformMask GetMaskFromData(object animData)
        {
            return animData switch
            {
                XPositionData => TransformMask.PosX,
                YPositionData => TransformMask.PosY,
                XRotationData => TransformMask.RotX,
                YRotationData => TransformMask.RotY,
                ZRotationData => TransformMask.RotZ,
                XScaleData => TransformMask.ScaleX,
                YScaleData => TransformMask.ScaleY,
                _ => TransformMask.None
            };
        }
    }

    // --- Вспомогательные структуры данных ---

    public enum TransformMask : int
    {
        None = 0,
        PosX = 1,
        PosY = 2,
        PosZ = 3,
        RotX = 4,
        RotY = 5,
        RotZ = 6,
        ScaleX = 7,
        ScaleY = 8,
        ScaleZ = 9
    }

    /// <summary>
    /// Job для применения результатов вычислений напрямую к Transform.
    /// Работает параллельно для всех объектов.
    /// </summary>
    [Unity.Burst.BurstCompile]
    public struct ApplyTransformJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<float4> Results;
        [ReadOnly] public NativeArray<int> ResultIndices;
        [ReadOnly] public NativeArray<TransformMask> Masks;

        public void Execute(int index, TransformAccess transform)
        {
            int resIdx = ResultIndices[index];
            float4 val = Results[resIdx];
            TransformMask mask = Masks[index];

            switch (mask)
            {
                case TransformMask.PosX:
                case TransformMask.PosY:
                case TransformMask.PosZ:
                    Vector3 pos = transform.localPosition;
                    if (mask == TransformMask.PosX) pos.x = val.x;
                    else if (mask == TransformMask.PosY) pos.y = val.x;
                    else pos.z = val.x;
                    transform.localPosition = pos;
                    break;

                case TransformMask.RotX:
                case TransformMask.RotY:
                case TransformMask.RotZ:
                    Vector3 rot = transform.localRotation.eulerAngles;
                    if (mask == TransformMask.RotX) rot.x = val.x;
                    else if (mask == TransformMask.RotY) rot.y = val.x;
                    else rot.z = val.x;
                    transform.localRotation = Quaternion.Euler(rot);
                    break;

                case TransformMask.ScaleX:
                case TransformMask.ScaleY:
                case TransformMask.ScaleZ:
                    Vector3 sc = transform.localScale;
                    if (mask == TransformMask.ScaleX) sc.x = val.x;
                    else if (mask == TransformMask.ScaleY) sc.y = val.x;
                    else sc.z = val.x;
                    transform.localScale = sc;
                    break;
            }
        }
    }

    /// <summary>
    /// Job для расчета интерполяции между ключами.
    /// </summary>
  [Unity.Burst.BurstCompile]
public struct AnimationJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<JobKeyframe> AllKeyframes;
    [ReadOnly] public NativeArray<JobTrackInfo> TrackInfos;
    public NativeArray<float4> Results;
    
    public double CurrentTime;
    public float SecondsPerTick; // Передается извне: 60.0 / (BPM * TicksPerBeat)

    public void Execute(int index)
    {
        JobTrackInfo track = TrackInfos[index];
        double localTime = CurrentTime - track.Offset;

        if (track.KeyCount == 0) return;

        int firstKeyIdx = track.StartKeyIndex;
        int lastKeyIdx = track.StartKeyIndex + track.KeyCount - 1;

        // 1. Краевые значения (Extrapolation: Clamp)
        if (localTime <= AllKeyframes[firstKeyIdx].Ticks)
        {
            Results[track.ResultIndex] = AllKeyframes[firstKeyIdx].Value;
            return;
        }

        if (localTime >= AllKeyframes[lastKeyIdx].Ticks)
        {
            Results[track.ResultIndex] = AllKeyframes[lastKeyIdx].Value;
            return;
        }

        // 2. Бинарный поиск пары ключей
        int low = firstKeyIdx;
        int high = lastKeyIdx;
        int foundIdx = low;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            if (AllKeyframes[mid].Ticks <= localTime)
            {
                foundIdx = mid;
                low = mid + 1;
            }
            else high = mid - 1;
        }

        var k1 = AllKeyframes[foundIdx];
        var k2 = AllKeyframes[foundIdx + 1];

        // 3. Расчет нормализованного времени t [0..1]
        double frameDurationTicks = k2.Ticks - k1.Ticks;
        if (frameDurationTicks <= 0) 
        {
            Results[track.ResultIndex] = k1.Value;
            return;
        }

        float t = (float)((localTime - k1.Ticks) / frameDurationTicks);
        
        // 4. Интерполяция
        Results[track.ResultIndex] = CalculateValue(k1, k2, t, (float)frameDurationTicks);
    }

    private float4 CalculateValue(JobKeyframe k1, JobKeyframe k2, float t, float ticksDt)
    {
        switch (k1.Interpolation)
        {
            case 0: // Linear
                return math.lerp(k1.Value, k2.Value, t);

            case 1: // Bezier
                // Длительность сегмента в секундах (нужна для масштабирования тангенсов)
                float dt = ticksDt * SecondsPerTick;

                // Контрольные точки времени (X)
                float x1 = k1.OutWeight;
                float x2 = 1f - k2.InWeight;

                // Находим параметр s через метод Ньютона
                float s = SolveCubicForT(x1, x2, t);

                // Контрольные точки значений (Y)
                float4 y0 = k1.Value;
                float4 y3 = k2.Value;

                // Стандартная формула Unity для контрольных точек:
                // P1 = P0 + (TangentOut * WeightOut * Duration)
                float4 y1 = y0 + (k1.OutTangent * (k1.OutWeight * dt));
                float4 y2 = y3 - (k2.InTangent * (k2.InWeight * dt));

                return CalculateBezier(y0, y1, y2, y3, s);

            case 2: // Hold
                return k1.Value;

            default:
                return math.lerp(k1.Value, k2.Value, t);
        }
    }

    private float SolveCubicForT(float x1, float x2, float t)
    {
        // Начальное приближение
        float s = t; 

        for (int i = 0; i < 8; i++)
        {
            float u = 1f - s;
            
            // Текущее значение X(s) для x0=0 и x3=1
            float f_s = 3f * u * u * s * x1 + 3f * u * s * s * x2 + s * s * s;

            // Производная dx/ds 
            float slope = 3f * u * u * x1 + 6f * u * s * (x2 - x1) + 3f * s * s * (1f - x2);

            if (math.abs(slope) < 1e-6f) break;

            s -= (f_s - t) / slope;
            s = math.clamp(s, 0f, 1f);
        }
        return s;
    }

    private float4 CalculateBezier(float4 p0, float4 p1, float4 p2, float4 p3, float s)
    {
        float u = 1f - s;
        float ss = s * s;
        float uu = u * u;
        
        return (uu * u * p0) + 
               (3f * uu * s * p1) + 
               (3f * u * ss * p2) + 
               (ss * s * p3);
    }
}

    public struct JobKeyframe
    {
        public double Ticks;
        public float4 Value;
        public int Interpolation;
        public float4 OutTangent;
        public float4 InTangent;
        public float OutWeight;
        public float InWeight;
    }

    public struct JobTrackInfo
    {
        public int StartKeyIndex;
        public int KeyCount;
        public double Offset;
        public int ResultIndex;
    }
}

public interface IAnimationApplyer

{
    public void Apply(Component target, float4 value);
}