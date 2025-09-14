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
        private SelectObjectController _selectObjectController;

        [Inject]
        public void Construct(GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage, SelectObjectController selectObjectController)
        {
            _gameEventBus = gameEventBus;
            _trackObjectStorage = trackObjectStorage;
            _selectObjectController = selectObjectController;
        }

        private void OnMouseDown()
        {
            _selectObjectController.Select(_trackObjectStorage.GetTrackObjectData(gameObject), UnityEngine.Input.GetKey(KeyCode.LeftShift));
        }
    }
}