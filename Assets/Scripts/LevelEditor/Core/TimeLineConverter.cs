using System.IO;
using TimeLine.Installers;
using TimeLine.LevelEditor.Core.MusicData;
using TimeLine.LevelEditor.Core.MusicOffset;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using Math = System.Math;

namespace TimeLine.LevelEditor.Core
{
    public class TimeLineConverter : MonoBehaviour
    {
        private MainObjects _mainObjects;
        private Main _main;
        private TimeLineScroll _timeLineScroll;
        private TimeLineSettings _timeLineSettings;
        private M_MusicData _mMusicData;
        private M_MusicOffsetData _mMusicOffsetData;
        private M_PlaybackState _mPlaybackState;
        private CurrentTimeMarkerRenderer _currentTimeMarkerRenderer;
        private M_AudioPlaybackService _audioPlaybackService;

        public static TimeLineConverter Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }


            _curve = new AnimationCurve();

            // Создаем ключевые кадры один раз
            var key1 = new UnityEngine.Keyframe(0f, 0f) { weightedMode = WeightedMode.Both };
            var key2 = new UnityEngine.Keyframe(1f, 0f) { weightedMode = WeightedMode.Both };

            _curve.AddKey(key1);
            _curve.AddKey(key2);
        }

        [Inject]
        private void Construct(MainObjects mainObjects, Main main, TimeLineScroll timeLineScroll,
            TimeLineSettings timeLineSettings, M_MusicData mMusicData, M_MusicOffsetData mMusicOffsetData,
            M_PlaybackState state, CurrentTimeMarkerRenderer currentTimeMarkerRenderer,
            M_AudioPlaybackService audioPlaybackService)
        {
            _mainObjects = mainObjects;
            _main = main;
            _timeLineSettings = timeLineSettings;
            _timeLineScroll = timeLineScroll;
            _mMusicData = mMusicData;
            _mMusicOffsetData = mMusicOffsetData;
            _mPlaybackState = state;
            _currentTimeMarkerRenderer = currentTimeMarkerRenderer;
            _audioPlaybackService = audioPlaybackService;
        }


        // Конвертация тиков в позицию X на UI
        public float TicksToPositionX(double ticks, float pan = 0)
        {
            // Конвертируем тики в секунды, затем в позицию
            double seconds = ticks * (60.0 / (_mMusicData.bpm * TICKS_PER_BEAT));
            return (float)(seconds * (_timeLineSettings.DistanceBetweenBeatLines + pan) *
                           (_mMusicData.bpm / 60.0));
        }
        
        public static BlobAssetReference<Unity.Physics.Collider> InstallConvexTriangle(float2[] points)
        {
            int count = points.Length;
            var vertices = new NativeArray<float3>(count * 2, Allocator.Temp);

            for (int i = 0; i < count; i++)
            {
                // Передняя грань (индексы 0 .. count-1)
                vertices[i] = new float3(points[i].x, points[i].y, 0.05f);

                // Задняя грань (индексы count .. 2*count-1)
                vertices[i + count] = new float3(points[i].x, points[i].y, -0.05f);
            }

            var hullParams = ConvexHullGenerationParameters.Default;
            BlobAssetReference<Unity.Physics.Collider> triangleCollider = ConvexCollider.Create(
                vertices,
                hullParams,
                CollisionFilter.Default
            );

            // vertices.Dispose(); // ПОДОЖДИ! 
            // ВАЖНО: Если ConvexCollider.Create не делает внутреннюю копию данных сразу, 
            // Dispose может вызвать краш. В Unity Physics BlobAsset обычно копирует данные, 
            // но лучше убедиться, что Dispose стоит после создания коллайдера.

            vertices.Dispose();
            // points.Dispose(); // Не диспозь points здесь, если они были созданы снаружи!

            return triangleCollider;
        }

        public float TicksToPositionXWithTimeLineOffset(double ticks, float pan = 0)
        {
            // Конвертируем тики в секунды, затем в позицию
            double seconds = ticks * (60.0 / (_mMusicData.bpm * TICKS_PER_BEAT));
            return (float)(seconds * (_timeLineSettings.DistanceBetweenBeatLines + pan) *
                           (_mMusicData.bpm / 60.0)) + _mainObjects.ContentRectTransform.offsetMin.x;
        }

        // Обратная конвертация: позиция X в тики (для Drag & Drop)
        public double PositionXToTicks(float positionX, float pan = 0)
        {
            // Обратная формула: позиция -> секунды -> тики
            double seconds = positionX / (pan * (_mMusicData.bpm / 60.0));
            return seconds * (_mMusicData.bpm * TICKS_PER_BEAT / 60.0);
        }

        public Vector2 CursorPosition(RectTransform canvasRectTransform = null) //todo пофиксить
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform != null
                    ? canvasRectTransform
                    : _mainObjects
                        .CanvasRectTransform, // RectTransform, в системе координат которого нужно получить точку  
                Mouse.current.position.ReadValue(), // точка в экранных координатах  
                _mainObjects.MainCamera, // для Overlay-канваса передаём null, для World Space — камеру  
                out var localPoint // результат: точка в локальных координатах RectTransform  
            );

            return localPoint;
            // return (new Vector2(UnityEngine.Input.mousePosition.x - _mainObjects.CanvasRectTransform.sizeDelta.x / 2,
            //     UnityEngine.Input.mousePosition.y - _mainObjects.CanvasRectTransform.sizeDelta.y / 2));
        }

        internal static Vector3 ConvertAnchorToWorldPosition(RectTransform rectTransform, Canvas canvas)
        {
            // Для World Space Canvas
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                return rectTransform.position;
            }

            // Для Screen Space Canvas
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
                canvas.worldCamera,
                rectTransform.position
            );

            Vector3 worldPosition;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rectTransform,
                screenPoint,
                canvas.worldCamera,
                out worldPosition
            );

            return worldPosition;
        }

        public double GetCursorBeatPosition(float pan, double offset = 0, RectTransform canvasOffset = null,
            RectTransform canvasCursor = null)
        {
            return (CursorPosition(canvasCursor).x - offset - (canvasOffset != null
                       ? canvasOffset.offsetMin.x
                       : _mainObjects.ContentRectTransform.offsetMin.x)) /
                   (_timeLineSettings.DistanceBetweenBeatLines + pan);
        }

        public float GetCursorBeatPosition(float offset = 0)
        {
            return (CursorPosition().x - offset - _mainObjects.ContentRectTransform.offsetMin.x) /
                   (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Zoom);
        }

        internal (Vector2 position, bool isInside) GetMousePosition(RectTransform rectTransformParentObject,
            Camera camera)
        {
            Vector2 mousePos = UnityEngine.Input.mousePosition;

            // 1. Конвертируем экранную точку в локальную
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransformParentObject,
                mousePos,
                camera,
                out Vector2 localPoint
            );

            // 2. Проверяем, входит ли локальная точка в прямоугольник RectTransform
            // rectTransformParentObject.rect — это область [-width/2, -height/2, width, height]
            bool actuallyInside = rectTransformParentObject.rect.Contains(localPoint);

            return (localPoint, actuallyInside);
        }

        public float GetTimeFromAnchorPosition(float anchorPosition, float pan = 0)
        {
            return GetTimeFromBeatPosition(anchorPosition / (_timeLineSettings.DistanceBetweenBeatLines + pan));
        }

        public double TicksCurrentTime()
        {
            if (_mPlaybackState.IsFirstPlaying)
            {
                return Math.Round(SecondsToTicks(_currentTimeMarkerRenderer.GetTime()));
            }
            else
            {
                return Math.Round(SecondsToTicks(_audioPlaybackService.Clip ? _audioPlaybackService.CurrentTime : 0) -
                                  SecondsToTicks(_mMusicOffsetData.Value));
            }
        }

        public float BeatPerSecondOffset() => _mMusicOffsetData.Value * (_mMusicData.bpm / 60);

        public float GetTimeFromBeatPosition(float beatPosition)
        {
            return 60 / _mMusicData.bpm * beatPosition;
        }

        public float GetBeatPositionFromTime(float time)
        {
            return (time * _mMusicData.bpm) / 60f;
        }

        public double SecondsToTicks(double seconds)
        {
            return seconds * (_mMusicData.bpm * TICKS_PER_BEAT / 60.0);
        }

        public double GetTimeInSeconds(double ticks)
        {
            return ticks * (60.0 / (_mMusicData.bpm * TICKS_PER_BEAT));
        }

        public float GetAnchorPositionFromTime(float time)
        {
            return GetAnchorPositionFromBeatPosition(time / (60 / _mMusicData.bpm)) +
                   _mainObjects.ContentRectTransform.offsetMin.x;
        }

        public float GetAnchorPositionFromTime2(float time)
        {
            return GetAnchorPositionFromBeatPosition(time / (60 / _mMusicData.bpm));
        }

        public float GetAnchorPositionFromBeatPosition(float time)
        {
            return GetAnchorPosition(time, _timeLineScroll.Zoom);
        }

        public float GetAnchorPositionFromBeatPosition(float time, float pan)
        {
            return GetAnchorPosition(time, pan);
        }


        [SerializeField] private AnimationCurve _curve;

        public float Interpolate(
            float start,
            float end,
            Keyframe.Keyframe current,
            Keyframe.Keyframe next,
            float t,
            Keyframe.Keyframe.InterpolationType interpolationType)
        {
            switch (interpolationType)
            {
                case Keyframe.Keyframe.InterpolationType.Hold:
                    return start;

                case Keyframe.Keyframe.InterpolationType.Linear:
                    return start + (end - start) * t;

                case Keyframe.Keyframe.InterpolationType.Bezier:
                    float dt = (float)TicksToSeconds(next.Ticks) - (float)TicksToSeconds(current.Ticks);

                    // 1. Определяем контрольные точки для Безье
                    // Веса (Weight) определяют "длину" касательной по горизонтали
                    float wOut = (float)current.OutWeight;
                    float wIn = (float)next.InWeight;

                    // Тангенсы (Tangent) - это производные (dy/dx)
                    float tanOut = (float)current.OutTangent;
                    float tanIn = (float)next.InTangent;

                    // Контрольные точки для X (времени)
                    float x0 = 0f;
                    float x1 = wOut;
                    float x2 = 1f - wIn;
                    float x3 = 1f;

                    // Контрольные точки для Y (значения)
                    float y0 = start;
                    float y1 = start + (tanOut * (wOut * dt));
                    float y2 = end - (tanIn * (wIn * dt));
                    float y3 = end;

                    // 2. Решаем кубическое уравнение, чтобы найти 's' (параметр кривой) для нашего 't'
                    float s = SolveCubicForT(x0, x1, x2, x3, t);

                    // 3. Вычисляем итоговое значение по формуле Безье для Y
                    return CalculateBezier(y0, y1, y2, y3, s);

                default:
                    return start + (end - start) * t;
            }
        }

// Стандартная формула кубического Безье
        private float CalculateBezier(float p0, float p1, float p2, float p3, float s)
        {
            float u = 1f - s;
            return u * u * u * p0 +
                   3f * u * u * s * p1 +
                   3f * u * s * s * p2 +
                   s * s * s * p3;
        }

// Поиск параметра s через итерации (т.к. время нелинейно при весах)
        private float SolveCubicForT(float x0, float x1, float x2, float x3, float t)
        {
            float s = t; // Начальное приближение
            for (int i = 0; i < 8; i++) // 8 итераций метода Ньютона обычно достаточно для float
            {
                float x = CalculateBezier(x0, x1, x2, x3, s);
                float slope = (3f * (1f - s) * (1f - s) * (x1 - x0) +
                               6f * (1f - s) * s * (x2 - x1) +
                               3f * s * s * (x3 - x2));

                if (Mathf.Abs(slope) < 1e-6) break;
                s -= (x - t) / slope;
                s = Mathf.Clamp01(s);
            }

            return s;
        }

        private void SafeInitializeCurve(float startValue = 0f, float endValue = 0f)
        {
            // 1. Полная переинициализация при повреждении
            if (_curve == null || _curve.length < 2)
            {
                _curve = new AnimationCurve();
                _curve.AddKey(new UnityEngine.Keyframe(0f, startValue) { weightedMode = WeightedMode.Both });
                _curve.AddKey(new UnityEngine.Keyframe(1f, endValue) { weightedMode = WeightedMode.Both });
                return;
            }

            // 2. Восстановление при некорректном количестве ключей
            var keys = _curve.keys;
            if (keys.Length != 2)
            {
                _curve.keys = new[]
                {
                    new UnityEngine.Keyframe(0f, startValue) { weightedMode = WeightedMode.Both },
                    new UnityEngine.Keyframe(1f, endValue) { weightedMode = WeightedMode.Both }
                };
                return;
            }

            // 3. Проверка валидности индексов
            try
            {
                _ = _curve[0]; // Принудительная проверка доступа
                _ = _curve[1];
            }
            catch
            {
                _curve.keys = new[]
                {
                    new UnityEngine.Keyframe(0f, startValue) { weightedMode = WeightedMode.Both },
                    new UnityEngine.Keyframe(1f, endValue) { weightedMode = WeightedMode.Both }
                };
            }
        }

        private void UpdateKeyframeDirect(int index, float time, float value, double tangent, double weight)
        {
            // Получаем ссылку на ключевой кадр и обновляем его
            UnityEngine.Keyframe key = _curve.keys[index];
            key.time = time;
            key.value = value;
            key.inTangent = (float)tangent;
            key.outTangent = (float)tangent;
            key.inWeight = (float)weight;
            key.outWeight = (float)weight;
            _curve.MoveKey(index, key);
        }
        //----------------------------------------------

// Вспомогательный метод для получения интервала
        private double GetTimeInterval(Keyframe.Keyframe current, Keyframe.Keyframe next)
        {
            // Если у ключей есть временные метки:
            // return next.Time - current.Time;
            double dt = next.Ticks - current.Ticks; // Если есть временные метки
            // Если временных меток нет, используйте дефолтное значение,
            // но лучше добавить временные метки для корректной работы весов
            return dt; // временное решение
        }


        public const double TICKS_PER_BEAT = 96.0; // 96 ticks per quarter note
        public const double SECONDS_IN_MINUTE = 60.0;
        public double SecondsPerTick => SECONDS_IN_MINUTE / (_mMusicData.bpm * TICKS_PER_BEAT);

        public double TicksToSeconds(double ticks)
        {
            return ticks * SecondsPerTick;
        }

        internal static AudioType GetAudioTypeFromPath(string path)
        {
            string extension = Path.GetExtension(path).ToLower();

            return extension switch
            {
                ".mp3" => AudioType.MPEG,
                ".wav" => AudioType.WAV,
                ".ogg" => AudioType.OGGVORBIS,
                ".aif" or ".aiff" => AudioType.AIFF,
                ".xm" => AudioType.XM,
                ".mod" => AudioType.MOD,
                ".s3m" => AudioType.S3M,
                ".it" => AudioType.IT,
                _ => AudioType.UNKNOWN
            };
        }

        public float GetAnchorPosition(float time, float pan)
        {
            return time * pan;
        }

        public static Vector3 GetPointBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * oneMinusT * p0 +
                3f * oneMinusT * oneMinusT * t * p1 +
                3f * oneMinusT * t * t * p2 +
                t * t * t * p3;
        }

        internal float GetAnchorPositionFromValue(float value, float pan)
        {
            float scrollFactor = pan;
            float position = value * scrollFactor; // ← Скролл НЕ добавляем!


            return position;
        }
    }
}