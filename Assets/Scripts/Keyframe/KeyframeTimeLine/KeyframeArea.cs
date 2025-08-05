using System;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class KeyframeArea : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;

        private TimeLineSettings _settings;
        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(TimeLineSettings settings,
            GameEventBus gameEventBus)
        {
            _settings = settings;
            _gameEventBus = gameEventBus;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo<SelectTrackObjectEvent>(OnSelectTrackObject);
        }

        public void OnSelectTrackObject(ref SelectTrackObjectEvent selectTrackObjectEvent)
        {
            UpdateArea(selectTrackObjectEvent.Track.trackObject.BeatDuraction);
        }

        private void UpdateArea(float duraction)
        {
            rect.sizeDelta = new Vector2(duraction * _settings.DistanceBetweenBeatLines, rect.sizeDelta.y);
            rect.anchoredPosition = new Vector2(rect.sizeDelta.x / 2, rect.anchoredPosition.y);
        }
    }
}