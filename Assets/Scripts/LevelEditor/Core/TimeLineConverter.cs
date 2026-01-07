using System;
using System.IO;
using NaughtyAttributes;
using TimeLine.Installers;
using TimeLine.LevelEditor.Core.MusicData;
using TimeLine.LevelEditor.Core.MusicOffset;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace TimeLine.TimeLine
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
            M_PlaybackState state, CurrentTimeMarkerRenderer currentTimeMarkerRenderer, M_AudioPlaybackService audioPlaybackService)
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
        
        internal (Vector2 position, bool isInside) GetMousePosition(RectTransform rectTransformParentObject, Camera camera)
        {
            // 1. Берем позицию мыши
            Vector2 mousePos = UnityEngine.Input.mousePosition;

            bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransformParentObject, // RectTransform родителя (например, TimeLineArea)
                mousePos, // Экранная позиция (Input.mousePosition)
                camera, // Камера (или null)
                out Vector2 position // Сюда запишется результат
            );

            return (position, isInside);
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

        public float GetAnchorPositionFromBeatPosition(float time)
        {
            return GetAnchorPosition(time, _timeLineSettings.DistanceBetweenBeatLines, _timeLineScroll.Zoom);
        }

        public float GetAnchorPositionFromBeatPosition(float time, float pan)
        {
            return GetAnchorPosition(time, _timeLineSettings.DistanceBetweenBeatLines, pan);
        }

        // public float Interpolate(
        //     float start, 
        //     float end, 
        //     Keyframe.Keyframe current, 
        //     Keyframe.Keyframe next, 
        //     float t)
        // {
        //     // Создаем AnimationCurve с двумя ключевыми кадрами
        //     AnimationCurve curve = new AnimationCurve();
        //
        //     // Устанавливаем ключевые кадры с их тангенсами и весами
        //     UnityEngine.Keyframe startKeyframe = new  UnityEngine.Keyframe(0f, start, (float)current.OutTangent, (float)current.OutTangent, (float)current.OutWeight, (float)current.OutWeight);
        //     UnityEngine.Keyframe endKeyframe = new  UnityEngine.Keyframe(1f, end, (float)next.InTangent, (float)next.InTangent, (float)next.InWeight, (float)next.InWeight);
        //
        //     // Добавляем ключевые кадры в кривую
        //     curve.AddKey(startKeyframe);
        //     curve.AddKey(endKeyframe);
        //
        //     // Устанавливаем режимы тангенсов
        //     curve.keys[0].weightedMode = WeightedMode.Both;
        //     curve.keys[1].weightedMode = WeightedMode.Both;
        //
        //     // Вычисляем значение на кривой
        //     return curve.Evaluate(Mathf.Clamp01(t));
        // }

        //----------------------------------------------
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
                    return start; // или end при t == 1f, если нужно "ступенчатое" поведение на границе

                case Keyframe.Keyframe.InterpolationType.Linear:
                    return Mathf.Lerp(start, end, t);

                case Keyframe.Keyframe.InterpolationType.Bezier:
                    // Используем AnimationCurve только для Bezier
                    float time1 = (float)TicksToSeconds(current.Ticks);
                    float time2 = (float)TicksToSeconds(next.Ticks);
                    float evalTime = time1 + t * (time2 - time1);

                    var key1 = new UnityEngine.Keyframe(
                        time: time1,
                        value: start,
                        inTangent: 0f,
                        outTangent: (float)current.OutTangent,
                        inWeight: 0f,
                        outWeight: (float)current.OutWeight
                    ) { weightedMode = WeightedMode.Out };

                    var key2 = new UnityEngine.Keyframe(
                        time: time2,
                        value: end,
                        inTangent: (float)next.InTangent,
                        outTangent: 0f,
                        inWeight: (float)next.InWeight,
                        outWeight: 0f
                    ) { weightedMode = WeightedMode.In };

                    var curve = new AnimationCurve(key1, key2);
                    return curve.Evaluate(evalTime);

                default:
                    return Mathf.Lerp(start, end, t);
            }
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

        public float GetAnchorPosition(float time, float distanceBetweenBeats, float pan)
        {
            return time * (distanceBetweenBeats + pan);
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
    }
}