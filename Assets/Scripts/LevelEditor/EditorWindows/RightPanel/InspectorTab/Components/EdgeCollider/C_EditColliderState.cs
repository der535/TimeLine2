using EventBus;
using TimeLine.EventBus.Events.Misc;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.General
{
    public class C_EditColliderState : MonoBehaviour
    {
        private bool _editState;

        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        public bool GetState() => _editState;

        public void Turn(bool active)
        {
            if(_editState == active) return;
            _gameEventBus.Raise(new TurnEditColliderEvent(active));
        }
        private void Start()
        {
            _gameEventBus.SubscribeTo((ref AddTrackObjectDataEvent data) =>
            {
                Turn(false);
            });

            _gameEventBus.SubscribeTo((ref RemoveTrackObjectDataEvent data) =>
            {
                Turn(false);
            });
            
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                Turn(false);
            });
            
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) =>
            {
                Turn(false);
            });

            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) =>
            {
                Turn(false);
            });
            
            
            _gameEventBus.SubscribeTo((ref TurnEditColliderEvent data) =>
            {
                _editState = data.IsEditing;
            });
            
        }
    }
}
