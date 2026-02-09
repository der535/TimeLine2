using EventBus;
using TimeLine.EventBus.Events.ValueEditor;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor.SelectController
{
    public class SelectConnectionController : MonoBehaviour
    {
        private NodeConnection _selectedConnection;

        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        public NodeConnection GetSelectedConnection()
        {
            return _selectedConnection;
        }
        
        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref SelectNodeConnectionEvent data) =>
            {
                Deselect();
                _selectedConnection = data.Node;
                _selectedConnection.SelectColor(true);
            });
        }

        public void Deselect()
        {
            if(_selectedConnection != null)
                _selectedConnection.SelectColor(false);
        }
    }
}