using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class ObjectSettings : MonoBehaviour
    {
        private GameEventBus _gameEventBus;
        private TrackObjectStorage _trackObjectStorage;

        [Inject]
        public void Construct(GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage)
        {
            _gameEventBus = gameEventBus;
            _trackObjectStorage = trackObjectStorage;
        }

        private void OnMouseDown()
        {
            _gameEventBus.Raise(new SelectObjectEvent(_trackObjectStorage.GetTrackObjectData(gameObject)));
        }
    }
}