using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class ObjectSettings : MonoBehaviour
    {
        private TrackObjectStorage _trackObjectStorage;
        private SelectObjectController _selectObjectController;
        private ActionMap _actionMap;

        [Inject]
        public void Construct(TrackObjectStorage trackObjectStorage, SelectObjectController selectObjectController, ActionMap actionMap)
        {
            _trackObjectStorage = trackObjectStorage;
            _selectObjectController = selectObjectController;
            _actionMap = actionMap;
        }

        private void OnMouseDown()
        {
            _selectObjectController.SelectMultiple(_trackObjectStorage.GetTrackObjectData(gameObject));
        }
    }
}