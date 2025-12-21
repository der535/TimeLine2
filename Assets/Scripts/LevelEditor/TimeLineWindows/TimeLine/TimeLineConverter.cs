using System;
using NaughtyAttributes;
using TimeLine.Installers;
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
            TimeLineSettings timeLineSettings)
        {
            _mainObjects = mainObjects;
            _main = main;
            _timeLineSettings = timeLineSettings;
            _timeLineScroll = timeLineScroll;
        }


        // Конвертация тиков в позицию X на UI
        public float TicksToPositionX(double ticks, float pan = 0)
        {
            // Конвертируем тики в секунды, затем в позицию
            double seconds = ticks * (60.0 / (_main.MusicData.bpm * Main.TICKS_PER_BEAT));
            return (float)(seconds * (_timeLineSettings.DistanceBetweenBeatLines + pan) *
                           (_main.MusicData.bpm / 60.0));
        }
        
        public float TicksToPositionXWithTimeLineOffset(double ticks, float pan = 0)
        {
            // Конвертируем тики в секунды, затем в позицию
            double seconds = ticks * (60.0 / (_main.MusicData.bpm * Main.TICKS_PER_BEAT));
            return (float)(seconds * (_timeLineSettings.DistanceBetweenBeatLines + pan) *
                           (_main.MusicData.bpm / 60.0)) + _mainObjects.ContentRectTransform.offsetMin.x;
        }

        // Обратная конвертация: позиция X в тики (для Drag & Drop)
        public double PositionXToTicks(float positionX)
        {
            // Обратная формула: позиция -> секунды -> тики
            double seconds = positionX / (_timeLineSettings.DistanceBetweenBeatLines * (_main.MusicData.bpm / 60.0));
            return seconds * (_main.MusicData.bpm * Main.TICKS_PER_BEAT / 60.0);
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
                   (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan);
        }

        public float GetTimeFromAnchorPosition(float anchorPosition, float pan = 0)
        {
            return GetTimeFromBeatPosition(anchorPosition / (_timeLineSettings.DistanceBetweenBeatLines + pan));
        }

        public float GetTimeFromBeatPosition(float beatPosition)
        {
            return 60 / _main.MusicData.bpm * beatPosition;
        }

        public float GetBeatPositionFromTime(float time)
        {
            return (time * _main.MusicData.bpm) / 60f;
        }

        public double SecondsToTicks(double seconds)
        {
            return seconds * (_main.MusicData.bpm * Main.TICKS_PER_BEAT / 60.0);
        }

        public double GetTimeInSeconds(double ticks)
        {
            return ticks * (60.0 / (_main.MusicData.bpm * Main.TICKS_PER_BEAT));
        }

        public float GetAnchorPositionFromTime(float time)
        {
            return GetAnchorPositionFromBeatPosition(time / (60 / _main.MusicData.bpm)) +
                   _mainObjects.ContentRectTransform.offsetMin.x;
        }

        public float GetAnchorPositionFromBeatPosition(float time)
        {
            return GetAnchorPosition(time, _timeLineSettings.DistanceBetweenBeatLines, _timeLineScroll.Pan);
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
        public double SecondsPerTick => SECONDS_IN_MINUTE / (_main.MusicData.bpm * TICKS_PER_BEAT);

        public double TicksToSeconds(double ticks)
        {
            return ticks * SecondsPerTick;
        }


        public float GetAnchorPosition(float time, float distanceBetweenBeats, float pan)
        {
            return time * (distanceBetweenBeats + pan);
        }
    }
}