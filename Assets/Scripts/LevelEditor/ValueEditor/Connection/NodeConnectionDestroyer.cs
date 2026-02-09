using TimeLine.LevelEditor.ValueEditor.SelectController;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor.Connection
{
    public class NodeConnectionDestroyer : MonoBehaviour
    {
        private SelectConnectionController _selectConnectionController;
        private ActionMap _actionMap;

        [Inject]
        private void Constructor(SelectConnectionController _connectionController, ActionMap actionMap)
        {
            _selectConnectionController = _connectionController;
            _actionMap = actionMap;

        }

        private void Start()
        {
            _actionMap.Editor.Enable();
            _actionMap.Editor.X.performed += (c) =>
            {
                if (_selectConnectionController.GetSelectedConnection() != null)
                {
                    var connection = _selectConnectionController.GetSelectedConnection();
                    connection.Disconnect();
                }
            };
            print(_actionMap);
        }
    }
}