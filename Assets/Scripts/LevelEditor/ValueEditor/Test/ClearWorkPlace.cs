using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor.Test
{
    public class ClearWorkPlace : MonoBehaviour
    {
        private NodeCreator _nodeCreator;
        private NodeConnector _nodeConnector;
        [Inject]
        private void Construct(NodeCreator nodeCreator, NodeConnector nodeConnector)
        {
            _nodeCreator = nodeCreator;
            _nodeConnector = nodeConnector;
        }

        public void Clear()
        {
            foreach (var node in _nodeCreator.GetNodes())
            {
                Destroy(node.gameObject);
            }

            foreach (var connection in _nodeConnector.GetConnections())
            {
                Destroy(connection.gameObject);
            }
            
            _nodeCreator.GetNodes().Clear();
            
            _nodeCreator.GetInitializedNodes().Clear();
            _nodeConnector.GetConnections().Clear();
        }
    }
}