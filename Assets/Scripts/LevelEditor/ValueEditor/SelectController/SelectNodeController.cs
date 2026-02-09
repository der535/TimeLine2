using EventBus;
using TimeLine.EventBus.Events.ValueEditor;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor.SelectController
{
    public class SelectNodeController : MonoBehaviour
    {
        private Node _selectedNode;

        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        public Node GetSelectedNode()
        {
            return _selectedNode;
        }
        
        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref SelectNodeEvent data) =>
            {
                Deselect();
                _selectedNode = data.Node;
                _selectedNode.GetSelectNode().SetSelected(true);
            });
        }

        public void Deselect()
        {
            if (_selectedNode != null)
            {
                _selectedNode.GetSelectNode().SetSelected(false);
                _selectedNode = null;
            }
        }
    }
}