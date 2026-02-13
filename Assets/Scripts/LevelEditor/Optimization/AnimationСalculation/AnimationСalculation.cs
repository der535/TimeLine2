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
using TimeLine.LevelEditor.ValueEditor.NodeLogic;
using TimeLine.TimeLine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using Zenject;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

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

        private List<Keyframe.Keyframe> _initializableKeyframes = new();

        private NativeArray<JobNode> _globalGraphNodes;
        private NativeArray<float> _globalGraphResults;
        private List<DynamicInputData> _allDynamicInputs = new();

        public struct DynamicInputData
        {
            public int globalOffset;
            public NodeLogic logic;
            public int outputIndex;
        }

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
            // Старые массивы
            if (_allKeyframes.IsCreated) _allKeyframes.Dispose();
            if (_trackInfos.IsCreated) _trackInfos.Dispose();
            if (_results.IsCreated) _results.Dispose();
            if (_transformMasks.IsCreated) _transformMasks.Dispose();
            if (_resultIndices.IsCreated) _resultIndices.Dispose();
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();

            // НОВЫЕ массивы (которые мы добавили для графов)
            if (_globalGraphNodes.IsCreated) _globalGraphNodes.Dispose();
            if (_globalGraphResults.IsCreated) _globalGraphResults.Dispose();
        }

        /// <summary>
        /// Основной цикл обновления анимаций
        /// </summary>
        public void UpdateTime(ref TickSmoothTimeEvent tick)
        {
            if (!_playAndStopButton._isPlaying) return;
            if (!_trackInfos.IsCreated || _trackInfos.Length == 0) return;

            for (int i = 0; i < _trackInfos.Length; i++)
            {
                var track = _trackInfos[i];
                double localTime = tick.Time - track.Offset;

                // Инициализируем только если время "зашло" внутрь трека (localTime >= 0)
                // Если localTime < 0, значит объект еще "не включился" по таймлайну
                if (localTime < 0) continue;

                // Поиск текущего ключа (бинарный поиск)
                int low = track.StartKeyIndex, high = track.StartKeyIndex + track.KeyCount - 1, found = low;
                while (low <= high) {
                    int mid = (low + high) / 2;
                    if (_allKeyframes[mid].Ticks <= localTime) { found = mid; low = mid + 1; }
                    else high = mid - 1;
                }

                int globalIdx = _allKeyframes[found].KeyframeGlobalIndex;
                var originalKey = _initializableKeyframes[globalIdx];

                // Проверка: инициализируем только если localTime находится в зоне действия ключа
                // И если этот конкретный ключ еще не "вспыхнул"
                if (!originalKey.IsInitialized)
                {
                    // Здесь можно добавить проверку на активность самого GameObject, если нужно:
                    // if (src.Track.cachedComponent.gameObject.activeInHierarchy) 

                    originalKey.Initialize.Invoke();
                    originalKey.IsInitialized = true;
            
                    // Лог для проверки:
                }
            }

            Profiler.BeginSample("AnimationSystem.UpdateTime");

            // 0. Обновляем динамические входы (включая InitializeLogic)
            foreach (var input in _allDynamicInputs)
            {
                // Теперь GetValue вернет то, что только что записал Invoke() выше
                float val = Convert.ToSingle(input.logic.GetValue(input.outputIndex));
                _globalGraphResults[input.globalOffset] = val;
            }

            // 1. Запуск Job
            var animJob = new AnimationJob
            {
                AllKeyframes = _allKeyframes,
                TrackInfos = _trackInfos,
                Results = _results,
                GraphNodes = _globalGraphNodes,
                GlobalGraphResults = _globalGraphResults,
                CurrentTime = tick.Time,
                SecondsPerTick = (float)TimeLineConverter.Instance.SecondsPerTick
            };


            JobHandle animHandle = animJob.Schedule(_trackInfos.Length, 64);

            // 2. Применение трансформов
            JobHandle finalHandle = animHandle;
            if (_transformAccessArray.isCreated && _transformAccessArray.length > 0)
            {
                var transJob = new ApplyTransformJob
                {
                    Results = _results,
                    ResultIndices = _resultIndices,
                    Masks = _transformMasks
                };
                finalHandle = transJob.Schedule(_transformAccessArray, animHandle);
            }

            finalHandle.Complete();

            Profiler.EndSample();

            // Оповещение и ручное применение
            for (int i = 0; i < _manualApplyers.Count; i++)
            {
                var item = _manualApplyers[i];
                item.applyer.Apply(item.target, _results[item.resIdx]);
            }

            TransformComponentStorage.InvokeAllComponents();
        }

        private List<NodeLogic> SortNodes(NodeLogic root)
        {
            List<NodeLogic> sortedNodes = new List<NodeLogic>();
            HashSet<NodeLogic> visited = new HashSet<NodeLogic>();
            FillSortedNodes(root, sortedNodes, visited);
            return sortedNodes;
        }

        private void FillSortedNodes(NodeLogic current, List<NodeLogic> sortedNodes, HashSet<NodeLogic> visited)
        {
            if (current == null || visited.Contains(current)) return;
            visited.Add(current);
            foreach (var inputPair in current.ConnectedInputs)
                FillSortedNodes(inputPair.Value.node, sortedNodes, visited);
            sortedNodes.Add(current);
        }

        /// <summary>
        /// Конвертация данных из объектной модели Unity в Native-массивы
        /// </summary>
        private void Bake()
        {
            var sourceTracks = _trackStorage.GetTracks();
            OnDestroy();
            _manualApplyers.Clear();
            _notifyTargets.Clear();
            _allDynamicInputs.Clear();

            int totalKeysCount = 0;
            int totalNodesCount = 0;
            foreach (var track in sourceTracks)
            {
                foreach (var key in track.Track.Keyframes)
                {
                    
                    totalKeysCount++;
                    if (key.GetData().Logic != null) totalNodesCount += SortNodes(key.GetData().Logic).Count;
                }
            }

            _allKeyframes = new NativeArray<JobKeyframe>(totalKeysCount, Allocator.Persistent);
            _trackInfos = new NativeArray<JobTrackInfo>(sourceTracks.Count, Allocator.Persistent);
            _results = new NativeArray<float4>(sourceTracks.Count, Allocator.Persistent);
            _globalGraphNodes = new NativeArray<JobNode>(totalNodesCount, Allocator.Persistent);
            _globalGraphResults = new NativeArray<float>(totalNodesCount * 4, Allocator.Persistent);

            List<Transform> transformsForJob = new();
            List<int> resultIndicesForJob = new();
            List<TransformMask> masksForJob = new();

            int currentKeyIndex = 0;
            int currentNodeIndex = 0;
            int currentResultOffset = 0;

            for (int i = 0; i < sourceTracks.Count; i++)
            {
                var src = sourceTracks[i];
                _trackInfos[i] = new JobTrackInfo
                {
                    StartKeyIndex = currentKeyIndex,
                    KeyCount = src.Track.Keyframes.Count,
                    Offset = src.Track.GetOffset() + src.TrackObject.StartTimeInTicks,
                    ResultIndex = i
                };

                for (int j = 0; j < src.Track.Keyframes.Count; j++)
                {
                    
                    
                    var kf = src.Track.Keyframes[j];
                    var data = kf.GetData();
                    var jobKey = new JobKeyframe
                    {
                        Ticks = kf.Ticks,
                        Interpolation = (int)kf.Interpolation,
                        OutTangent = new float4((float)kf.OutTangent),
                        InTangent = new float4((float)kf.InTangent),
                        OutWeight = (float)kf.OutWeight,
                        InWeight = (float)kf.InWeight,
                        GraphStartIndex = -1
                    };

                    
                    jobKey.KeyframeGlobalIndex = _initializableKeyframes.Count;
                    _initializableKeyframes.Add(kf);
                    
                    if (!string.IsNullOrEmpty(data.Graph))
                    {
                        var sortedNodes = SortNodes(data.Logic);
                        jobKey.GraphStartIndex = currentNodeIndex;
                        jobKey.GraphLength = sortedNodes.Count;
                        jobKey.ResultOffset = currentResultOffset;

                        for (int n = 0; n < sortedNodes.Count; n++)
                        {
                            var node = sortedNodes[n];
                            var inst = new JobNode();
                            MapLogicToJob(node, ref inst);

                            var (idxA, portA) = GetInputConnection(node, 0, sortedNodes);
                            var (idxB, portB) = GetInputConnection(node, 1, sortedNodes);
                            inst.InputIdxA = idxA;
                            inst.InputPortA = portA;
                            inst.InputIdxB = idxB;
                            inst.InputPortB = portB;

                            _globalGraphNodes[currentNodeIndex] = inst;

                            int nodeBaseIdx = currentResultOffset + (n * 4);
                            if (node is ComponentFieldLogic cf)
                                _allDynamicInputs.Add(new DynamicInputData
                                    { globalOffset = nodeBaseIdx, logic = cf, outputIndex = 0 });
                            else if (node is PlayerPositionLogic pp)
                                for (int p = 0; p < 3; p++)
                                    _allDynamicInputs.Add(new DynamicInputData
                                        { globalOffset = nodeBaseIdx + p, logic = pp, outputIndex = p });

                            if (node is RandomRangeLogic rangeLogic)
                            {
                                _allDynamicInputs.Add(new DynamicInputData { 
                                    globalOffset = nodeBaseIdx, 
                                    logic = rangeLogic, 
                                    outputIndex = 0 
                                });
                            }
                            
                            if (node is InitializeLogic initLogic)
                            {
                                _allDynamicInputs.Add(new DynamicInputData { 
                                    globalOffset = nodeBaseIdx, 
                                    logic = initLogic, 
                                    outputIndex = 0 
                                });
                            }
                            
                            if (inst.Op == OpCode.Constant) _globalGraphResults[nodeBaseIdx] = inst.Value;
                            if (inst.Op == OpCode.DirectPass && idxA == -1)
                                _globalGraphResults[nodeBaseIdx] = inst.Value;

                            currentNodeIndex++;
                        }

                        currentResultOffset += sortedNodes.Count * 4;
                    }
                    else jobKey.Value = data.PackDataToFloat4();

                    _allKeyframes[currentKeyIndex++] = jobKey;
                }

                (int nodeIdx, int portIdx) GetInputConnection(NodeLogic node, int portIndex,
                    List<NodeLogic> sortedNodes)
                {
                    if (node.ConnectedInputs.TryGetValue(portIndex, out var conn))
                        return (sortedNodes.IndexOf(conn.node), conn.outputIndex);
                    return (-1, 0);
                }

                var mask = GetMaskFromData(src.Track.GetAnimationData());
                if (mask != TransformMask.None && src.Track.cachedComponent != null)
                {
                    transformsForJob.Add(src.Track.cachedComponent.transform);
                    resultIndicesForJob.Add(i);
                    masksForJob.Add(mask);
                    _notifyTargets.Add(src.Track.cachedComponent);
                }
                else if (src.Track.cachedComponent != null)
                {
                    _manualApplyers.Add((src.Track.GetApplyer().Item1, src.Track.cachedComponent, i));
                }
            }

            _transformAccessArray = new TransformAccessArray(transformsForJob.ToArray());
            _resultIndices = new NativeArray<int>(resultIndicesForJob.ToArray(), Allocator.Persistent);
            _transformMasks = new NativeArray<TransformMask>(masksForJob.ToArray(), Allocator.Persistent);
        }

        private int GetInputIndexRelative(NodeLogic node, int portIndex, List<NodeLogic> sortedNodes)
        {
            if (node.ConnectedInputs.TryGetValue(portIndex, out var connection))
                return sortedNodes.IndexOf(connection.node);
            return -1;
        }

        private void MapLogicToJob(NodeLogic logic, ref JobNode inst)
        {
            if (logic is FloatLogic fl) { inst.Op = OpCode.Constant; inst.Value = fl.Value; }
            else if (logic is ComponentFieldLogic) { inst.Op = OpCode.Input; }
            else if (logic is PlayerPositionLogic) { inst.Op = OpCode.PlayerPos; }
            else if (logic is RandomRangeLogic){ inst.Op = OpCode.Input;  }
            else if (logic is InitializeLogic) { inst.Op = OpCode.Input;  } // Просто Input, данные придут извне
            else if (logic is AddLogic) { inst.Op = OpCode.Add; }
            else if (logic is OutputLogic outL) { inst.Op = OpCode.DirectPass; inst.Value = (float)outL.ManualValues[0]; }
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
    [BurstCompile]
    public struct AnimationJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<JobKeyframe> AllKeyframes;
        [ReadOnly] public NativeArray<JobTrackInfo> TrackInfos;
        [ReadOnly] public NativeArray<JobNode> GraphNodes;
        [NativeDisableParallelForRestriction] public NativeArray<float> GlobalGraphResults;
        public NativeArray<float4> Results;
        public double CurrentTime;
        public float SecondsPerTick;

        public void Execute(int index)
        {
            var track = TrackInfos[index];
            double localTime = CurrentTime - track.Offset;
            if (track.KeyCount == 0) return;

            int low = track.StartKeyIndex, high = track.StartKeyIndex + track.KeyCount - 1, found = low;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (AllKeyframes[mid].Ticks <= localTime)
                {
                    found = mid;
                    low = mid + 1;
                }
                else high = mid - 1;
            }

            if (localTime <= AllKeyframes[track.StartKeyIndex].Ticks)
            {
                Results[track.ResultIndex] = ResolveValue(AllKeyframes[track.StartKeyIndex]);
                return;
            }

            if (localTime >= AllKeyframes[track.StartKeyIndex + track.KeyCount - 1].Ticks)
            {
                Results[track.ResultIndex] = ResolveValue(AllKeyframes[track.StartKeyIndex + track.KeyCount - 1]);
                return;
            }

            var k1 = AllKeyframes[found];
            var k2 = AllKeyframes[found + 1];
            float t = (float)((localTime - k1.Ticks) / (k2.Ticks - k1.Ticks));
            Results[track.ResultIndex] = CalculateInterpolation(k1, k2, ResolveValue(k1), ResolveValue(k2), t,
                (float)(k2.Ticks - k1.Ticks));
        }

        private float4 ResolveValue(JobKeyframe kf)
        {
            if (kf.GraphStartIndex == -1) return kf.Value;
            for (int i = 0; i < kf.GraphLength; i++)
            {
                var node = GraphNodes[kf.GraphStartIndex + i];
                int resIdx = kf.ResultOffset + (i * 4);
                float a = (node.InputIdxA != -1)
                    ? GlobalGraphResults[kf.ResultOffset + (node.InputIdxA * 4) + node.InputPortA]
                    : node.Value;
                float b = (node.InputIdxB != -1)
                    ? GlobalGraphResults[kf.ResultOffset + (node.InputIdxB * 4) + node.InputPortB]
                    : 0;

                switch (node.Op)
                {
                    case OpCode.Constant: GlobalGraphResults[resIdx] = node.Value; break;
                    case OpCode.Add: GlobalGraphResults[resIdx] = a + b; break;
                    case OpCode.Multiply: GlobalGraphResults[resIdx] = a * b; break;
                    case OpCode.DirectPass: GlobalGraphResults[resIdx] = (node.InputIdxA != -1) ? a : node.Value; break;
                }
            }

            return new float4(GlobalGraphResults[kf.ResultOffset + (kf.GraphLength - 1) * 4], 0, 0, 0);
        }

        private float4 CalculateInterpolation(JobKeyframe k1, JobKeyframe k2, float4 v1, float4 v2, float t,
            float dtTicks)
        {
            if (k1.Interpolation == 0) return math.lerp(v1, v2, t);
            if (k1.Interpolation == 2) return v1;
            float dt = dtTicks * SecondsPerTick;
            float s = t; // Упрощенный поиск T для Безье
            for (int i = 0; i < 4; i++)
            {
                float u = 1 - s;
                float f = 3 * u * u * s * k1.OutWeight + 3 * u * s * s * (1 - k2.InWeight) + s * s * s - t;
                float d = 3 * u * u * k1.OutWeight + 6 * u * s * (1 - k2.InWeight - k1.OutWeight) +
                          3 * s * s * (1 - (1 - k2.InWeight));
                if (math.abs(d) < 1e-5f) break;
                s -= f / d;
            }

            s = math.clamp(s, 0, 1);
            float u1 = 1 - s;
            return (u1 * u1 * u1 * v1) + (3 * u1 * u1 * s * (v1 + k1.OutTangent * k1.OutWeight * dt)) +
                   (3 * u1 * s * s * (v2 - k2.InTangent * k2.InWeight * dt)) + (s * s * s * v2);
        }
    }
    public struct JobKeyframe
    {
        public double Ticks;
        public float4 Value; // Если GraphIndex == -1, берем это
        public int Interpolation;

        // --- Поля для интеграции графов ---
        public int GraphStartIndex; // Индекс начала инструкций в глобальном массиве
        public int GraphLength; // Количество нод в графе
        public int ResultOffset; // Где в глобальном ResultsBuffer лежат результаты этого графа


        public float4 OutTangent;
        public float4 InTangent;
        public float OutWeight;
        public float InWeight;
        
        public int KeyframeGlobalIndex; // Индекс ключа в общем списке для обратной связи
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