using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class DeselectObject : MonoBehaviour
    {
        private GameEventBus _gameEventBus;
        private SelectObjectController _selectObjectController;

        [Inject]
        void Construct(GameEventBus gameEventBus, SelectObjectController selectObjectController)
        {
            _gameEventBus = gameEventBus;
            _selectObjectController = selectObjectController;
        }
        
        public void Deselect()
        {
            _selectObjectController.DeselectAll();
            _gameEventBus.Raise(new DeselectAllObjectEvent());
        }
    }
}
