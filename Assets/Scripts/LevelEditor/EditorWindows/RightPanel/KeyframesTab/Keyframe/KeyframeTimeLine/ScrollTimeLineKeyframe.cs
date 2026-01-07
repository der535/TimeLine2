using System;
using EventBus;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace TimeLine.Keyframe.KeyframeTimeLine
{
    public class ScrollTimeLineKeyframe : MonoBehaviour
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private RectTransform content;
        [SerializeField] private TimeLineKeyframeScroll scroll;
        [Space] [SerializeField] private float offset;

        private GameEventBus _gameEventBus;
        private TimeLineConverter _timeLineConverter;
        private float _oldPan;

        [Inject]
        void Construct(GameEventBus eventBus, TimeLineConverter timeLineConverter)
        {
            _gameEventBus = eventBus;
            _timeLineConverter = timeLineConverter;
        }

        public void SetPosition(float position)
        {
            content.offsetMin = new Vector2(position, 0); //Left
            content.offsetMax = new Vector2(position, 0); //Right
        }

        public void AddPosition(float position)
        {
            content.offsetMin += new Vector2(position, 0); //Left
            content.offsetMax += new Vector2(position, 0); //Right
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo(
                (ref ScrollTimeLineKeyframeEvent scrollEvent) => { AddPosition(scrollEvent.ScrollOffset); }, 1);

            _gameEventBus.SubscribeTo((ref OldPanEvent oldPanEvent) => _oldPan = oldPanEvent.OldPanOffset, 1);
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.PanEvent _) =>
            {
                float curPos = (float)_timeLineConverter.GetCursorBeatPosition(_oldPan,0, content, panel);
                SetPosition(-(_timeLineConverter.GetAnchorPositionFromBeatPosition(curPos, scroll.Zoom) -
                              _timeLineConverter.CursorPosition(panel).x));
            }, 1);
        }
        


    }
}