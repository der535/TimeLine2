using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class KeyframeArea : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        [SerializeField] private TimeLineKeyframeScroll _timeLineKeyframeScroll;

        private TrackObjectData selectedTrackObjectData;

        private TimeLineSettings _settings;
        private GameEventBus _gameEventBus;
        private TimeLineConverter _timeLineConverter;

        private float _oldPan;

        [Inject]
        private void Construct(TimeLineSettings settings,
            GameEventBus gameEventBus, TimeLineConverter timeLineConverter)
        {
            _settings = settings;
            _gameEventBus = gameEventBus;
            _timeLineConverter = timeLineConverter;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) => OnSelectTrackObject(data.Track));
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.PanEvent _) => OnSelectTrackObject(selectedTrackObjectData));
            
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => Clear());

        }

        public void OnSelectTrackObject(TrackObjectData trackObjectData)
        {
            if (trackObjectData == null) return;
            selectedTrackObjectData = trackObjectData;
            UpdateArea((float)trackObjectData.trackObject.BeatDuraction);
        }

        private void Clear()
        {
            selectedTrackObjectData = null;
            UpdateArea(0);
        }

        private void UpdateArea(float duraction)
        {
            rect.sizeDelta = new Vector2(duraction * (_settings.DistanceBetweenBeatLines + _timeLineKeyframeScroll.Pan), rect.sizeDelta.y);
            rect.anchoredPosition = new Vector2(rect.sizeDelta.x / 2, rect.anchoredPosition.y);
        }
    }
}