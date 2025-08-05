using System;
using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using UnityEngine.InputSystem.iOS;
using Zenject;

namespace TimeLine
{
    public class KeyframeTimeLine : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;

        private TrackObject _trackObject;

        private Main _main;
        private GameEventBus _gameEventBus;
        private TimeLineSettings _timeLineSettings;

        [Inject]
        private void Construct(Main main, GameEventBus gameEventBus, TimeLineSettings timeLineSettings)
        {
            _main = main;
            _gameEventBus = gameEventBus;
            _timeLineSettings = timeLineSettings;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo<SmoothTimeEvent>(OnTimeChangedSmooth);
            _gameEventBus.SubscribeTo<SelectTrackObjectEvent>(OnSelectTrackObject);
            _gameEventBus.SubscribeTo<DragTrackObjectEvent>(OnDragTrackObject);
        }

        public void OnTimeChangedSmooth(ref SmoothTimeEvent smoothTimeEvent)
        {
            UpdatePosition(smoothTimeEvent.Time);
        }

        public void OnSelectTrackObject(ref SelectTrackObjectEvent selectTrackObjectEvent)
        {
            _trackObject = selectTrackObjectEvent.Track.trackObject;
            UpdatePosition(_main.CurrentTime);
        }

        public void OnDragTrackObject(ref DragTrackObjectEvent dragTrackObjectEvent)
        {
            UpdatePosition(_main.CurrentTime);
        }

        private void UpdatePosition(float time)
        {
            if (_trackObject)
                rect.anchoredPosition =
                    new Vector2(
                        (time - _trackObject.StartTime) * _timeLineSettings.DistanceBetweenBeatLines *
                        (_main.MusicDataSo.bpm / 60), rect.anchoredPosition.y);
        }
    }
}