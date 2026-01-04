using NaughtyAttributes;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine
{
    public class TimeLineMarker : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image image;

        private double _markerTime;
        private TimeLineConverter _timeLineConverter;
        private TimeLineScroll _timeLineScroll;
        
        [Inject]
        private void Constructor(TimeLineConverter timeLineConverter, TimeLineScroll timeLineScroll)
        {
            _timeLineConverter = timeLineConverter;
            _timeLineScroll = timeLineScroll;
        }
        
        internal void Setup(double time, Color color)
        {
            image.color = color;
            _markerTime = time;
        }

        internal void UpdateTime(float time)
        {
            _markerTime = time;
        }
        
        internal void UpdatePosition()
        {
            rectTransform.anchoredPosition = new Vector2(_timeLineConverter.TicksToPositionXWithTimeLineOffset(_markerTime, _timeLineScroll.Zoom), 0);
        }
    }
}