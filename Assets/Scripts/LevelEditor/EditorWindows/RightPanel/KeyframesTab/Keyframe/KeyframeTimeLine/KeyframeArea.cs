using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class KeyframeArea : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        [FormerlySerializedAs("_timeLineKeyframeScroll")] [SerializeField] private TimeLineKeyframeZoom timeLineKeyframeZoom;

        private TrackObjectPacket _selectedTrackObjectPacket;

        private TimeLineSettings _settings;
        private GameEventBus _gameEventBus;
        private TimeLineConverter _timeLineConverter;
        private MainObjects _mainObjects;

        private float _oldPan;
        private TrackObjectPacket _savedTrackObjectPacket;

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
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                if(data.UpdateVisual)
                    OnSelectTrackObject(data.Tracks[^1]);
            });
            _gameEventBus.SubscribeTo((ref TrackObjectChangeDuractionEvent data) => OnSelectTrackObject(_savedTrackObjectPacket));
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => OnSelectTrackObject(data.SelectedObjects[^1]));
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.KeyframeZoomEvent _) =>
            {
                OnSelectTrackObject(_selectedTrackObjectPacket);
            });
            
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) => Clear());

        }

        public void OnSelectTrackObject(TrackObjectPacket trackObjectPacket)
        {
            if (trackObjectPacket == null) return;
            
            _selectedTrackObjectPacket = trackObjectPacket;
            _savedTrackObjectPacket = trackObjectPacket;

            UpdateArea((float)(trackObjectPacket.components.Data.TimeDurationInTicks / TimeLineConverter.TICKS_PER_BEAT));
        }

        private void Clear()
        {
            _savedTrackObjectPacket = null;
            _selectedTrackObjectPacket = null;
            UpdateArea(0);
        }

        private void UpdateArea(float duraction)
        {
            // float rootOffset = _mainObjects.KeyframeScrollView.offsetMin.x - _mainObjects.KeyframeVerticalLayoutGroup.padding.left / 2f;

            rect.sizeDelta = new Vector2(duraction * (timeLineKeyframeZoom.Zoom), rect.sizeDelta.y);
            rect.anchoredPosition = new Vector2((rect.sizeDelta.x / 2), rect.anchoredPosition.y);
        }
    }
}