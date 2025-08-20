using TimeLine.Installers;
using UnityEngine;
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

        public Vector2 CursorPosition()
        {
            return (new Vector2(UnityEngine.Input.mousePosition.x - _mainObjects.CanvasRectTransform.sizeDelta.x / 2,
                UnityEngine.Input.mousePosition.y - _mainObjects.CanvasRectTransform.sizeDelta.y / 2));
        }

        public float GetCursorBeatPosition(float pan, float offset = 0)
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


        public float GetAnchorPositionFromTime(float time)
        {
            return GetAnchorPositionFromBeatPosition(time / (60 / _main.MusicDataSo.bpm)) +
                   _mainObjects.ContentRectTransform.offsetMin.x;
        }

        public float GetAnchorPositionFromBeatPosition(float time)
        {
            return GetAnchorPosition(time, _timeLineSettings.DistanceBetweenBeatLines, _timeLineScroll.Pan);
        }

        public float GetAnchorPosition(float time, float distanceBetweenBeats, float pan)
        {
            return time * (distanceBetweenBeats + pan);
        }
    }
}