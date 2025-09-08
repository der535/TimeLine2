using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class DeselectObject : MonoBehaviour
    {
        private GameEventBus _gameEventBus;

        [Inject]
        void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        public void Deselect()
        {
            _gameEventBus.Raise(new DeselectObjectEvent());
        }
    }
}
