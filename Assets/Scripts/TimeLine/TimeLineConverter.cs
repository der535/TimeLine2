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
        public float TicksToPositionX(double ticks)
        {
            // Конвертируем тики в секунды, затем в позицию
            double seconds = ticks * (60.0 / (_main.MusicDataSo.bpm * Main.TICKS_PER_BEAT));
            return (float)(seconds * _timeLineSettings.DistanceBetweenBeatLines * (_main.MusicDataSo.bpm / 60.0));
        }

        // Обратная конвертация: позиция X в тики (для Drag & Drop)
        public double PositionXToTicks(float positionX)
        {
            // Обратная формула: позиция -> секунды -> тики
            double seconds = positionX / (_timeLineSettings.DistanceBetweenBeatLines * (_main.MusicDataSo.bpm / 60.0));
            return seconds * (_main.MusicDataSo.bpm * Main.TICKS_PER_BEAT / 60.0);
        }

        public Vector2 CursorPosition() //todo пофиксить
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(  
                _mainObjects.CanvasRectTransform, // RectTransform, в системе координат которого нужно получить точку  
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

        public double GetCursorBeatPosition(float pan, double offset = 0)
        {
            return (CursorPosition().x - offset - _mainObjects.ContentRectTransform.offsetMin.x) /
                   (_timeLineSettings.DistanceBetweenBeatLines + pan);
        }

        public float GetCursorBeatPosition(float offset = 0)
        {
            return (CursorPosition().x - offset - _mainObjects.ContentRectTransform.offsetMin.x) /
                   (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan);
        }

        public float GetTimeFromAnchorPosition(float anchorPosition)
        {
            return GetTimeFromBeatPosition(anchorPosition / _timeLineSettings.DistanceBetweenBeatLines);
        }

        public float GetTimeFromBeatPosition(float beatPosition)
        {
            return 60 / _main.MusicDataSo.bpm * beatPosition;
        }

        public float GetBeatPositionFromTime(float time)
        {
            return (time * _main.MusicDataSo.bpm) / 60f;
        }
        public double SecondsToTicks(double seconds)
        {
            return seconds * (_main.MusicDataSo.bpm * Main.TICKS_PER_BEAT / 60.0);
        }
        
        public double GetTimeInSeconds(double ticks)
        {
            return ticks * (60.0 / (_main.MusicDataSo.bpm * Main.TICKS_PER_BEAT));
        }
        
        public float GetAnchorPositionFromTime(float time)
        {
            return GetAnchorPositionFromBeatPosition(time / (60 / _main.MusicDataSo.bpm)) +
                   _mainObjects.ContentRectTransform.offsetMin.x;
        }        

        public float GetAnchorPositionFromBeatPosition(float time)
        {
            return GetAnchorPosition(time, _timeLineSettings.DistanceBetweenBeatLines, _timeLineScroll.Pan);
        }
        public const double TICKS_PER_BEAT = 96.0; // 96 ticks per quarter note
        public const double SECONDS_IN_MINUTE = 60.0;
        public double SecondsPerTick => SECONDS_IN_MINUTE / (_main.MusicDataSo.bpm * TICKS_PER_BEAT);
        
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