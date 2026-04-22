using EventBus;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.LevelEditor.Core;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine
{   
    /// <summary>
    /// Предназначен для прокрутки таймлайна с ключевыми кадрами
    /// </summary>
    public class ScrollTimeLineKeyframe : MonoBehaviour
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private RectTransform content;
        [SerializeField] private WindowsFocus focus;
        [FormerlySerializedAs("scroll")] [SerializeField] private TimeLineKeyframeZoom zoom;

        private GameEventBus _gameEventBus;
        private TimeLineConverter _timeLineConverter;
        private float _oldPan;

        [Inject]
        void Construct(GameEventBus eventBus, TimeLineConverter timeLineConverter)
        {
            _gameEventBus = eventBus;
            _timeLineConverter = timeLineConverter;
        }
        
        private void Start()
        {
            _gameEventBus.SubscribeTo(
                (ref ScrollTimeLineKeyframeEvent scrollEvent) => { AddPosition(scrollEvent.ScrollOffset); }, 1);

            _gameEventBus.SubscribeTo((ref KeyframeOldZoomEvent oldPanEvent) => _oldPan = oldPanEvent.OldZoom, 1);
            
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.KeyframeZoomEvent _) =>
            {
                float curPos = (float)_timeLineConverter.GetCursorBeatPosition(_oldPan,0, content, panel);
                SetPosition(-(_timeLineConverter.GetAnchorPositionFromBeatPosition(curPos, zoom.Zoom) -
                              _timeLineConverter.CursorPosition(panel).x));
            }, 5);
        }
        
        /// <summary>
        /// Устанавливает конкретную позицию
        /// </summary>
        /// <param name="position">Точная позиция</param>
        public void SetPosition(float position)
        {
            content.offsetMin = new Vector2(position, 0); //Left
            content.offsetMax = new Vector2(position, 0); //Right
            _gameEventBus.Raise(new ScrollTimeLineKeyframeEvent());
        }
        /// <summary>
        /// Прибавляет к текущей позиции
        /// </summary>
        /// <param name="position">Долбовляемая позиция</param>
        public void AddPosition(float position)
        {
            content.offsetMin += new Vector2(position, 0); //Left
            content.offsetMax += new Vector2(position, 0); //Right
        }
    }
}