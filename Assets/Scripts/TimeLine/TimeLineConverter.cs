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
        }

        [Inject]
        private void Construct(MainObjects mainObjects, Scroll scroll, Main main, TimeLineScroll timeLineScroll,
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

        public double GetCursorBeatPosition(float pan, double offset = 0, RectTransform canvasOffset = null, RectTransform canvasCursor = null)
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
            return GetAnchorPosition(time,  _timeLineSettings.DistanceBetweenBeatLines, _timeLineScroll.Pan);
        }
        
        public float GetAnchorPositionFromBeatPosition(float time, float pan)
        {
            return GetAnchorPosition(time,  _timeLineSettings.DistanceBetweenBeatLines, pan);
        }
        
        public float Interpolate(
            float start, 
            float end, 
            Keyframe.Keyframe current, 
            Keyframe.Keyframe next, 
            float t)
        {
            // Нормализуем t, предполагая, что он от 0 до 1 (как в оригинальном коде)
            float u = Mathf.Clamp01(t);
    
            // Получаем значения
            float v0 = start;
            float v1 = end;
    
            // Тангенсы
            float m0 = (float)current.OutTangent; // OutTangent для первого ключа
            float m1 = (float)next.InTangent;     // InTangent для второго ключа
    
            // Весы
            float w0 = (float)current.OutWeight; // OutWeight для первого ключа
            float w1 = (float)next.InWeight;     // InWeight для второго ключа
    
            // Время между кадрами (в оригинале 1 - 0 = 1)
            float dt = 1.0f;
    
            // Эффективные тангенсы с учётом весов
            float m0_eff = m0 * dt * w0;
            float m1_eff = m1 * dt * w1;
    
            // Кубическая эрмитова интерполяция
            float u2 = u * u;
            float u3 = u2 * u;
    
            float h00 = 2 * u3 - 3 * u2 + 1;      // (2u^3 - 3u^2 + 1)
            float h10 = u3 - 2 * u2 + u;          // (u^3 - 2u^2 + u)
            float h01 = -2 * u3 + 3 * u2;         // (-2u^3 + 3u^2)
            float h11 = u3 - u2;                  // (u^3 - u^2)
    
            return h00 * v0 + h10 * m0_eff + h01 * v1 + h11 * m1_eff;
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