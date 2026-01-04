using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
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
        private MainObjects _mainObjects;

        private float _oldPan;

        [Inject]
        private void Construct(TimeLineSettings settings,
            GameEventBus gameEventBus, TimeLineConverter timeLineConverter, MainObjects mainObjects)
        {
            _settings = settings;
            _gameEventBus = gameEventBus;
            _timeLineConverter = timeLineConverter;
            _mainObjects = mainObjects;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) => OnSelectTrackObject(data.Tracks[^1]));
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => OnSelectTrackObject(data.SelectedObjects[^1]));
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.PanEvent _) => OnSelectTrackObject(selectedTrackObjectData));
            
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) => Clear());

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
            // float rootOffset = _mainObjects.KeyframeScrollView.offsetMin.x - _mainObjects.KeyframeVerticalLayoutGroup.padding.left / 2f;

            rect.sizeDelta = new Vector2(duraction * (_settings.DistanceBetweenBeatLines + _timeLineKeyframeScroll.Pan), rect.sizeDelta.y);
            rect.anchoredPosition = new Vector2((rect.sizeDelta.x / 2), rect.anchoredPosition.y);
        }
    }
}