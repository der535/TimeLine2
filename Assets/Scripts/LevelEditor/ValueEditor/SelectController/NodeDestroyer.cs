using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor.SelectController
{
    public class NodeDestroyer : MonoBehaviour
    {
        private SelectNodeController _selectNodeController;
        private NodeCreator _nodeCreator;
        private ActionMap _actionMap;

        [Inject]
        private void Constructor(SelectNodeController selectNodeController, ActionMap actionMap, NodeCreator nodeCreator)
        {
            _actionMap = actionMap;
            _selectNodeController = selectNodeController;
            _nodeCreator = nodeCreator;
        }

        private void Start()
        {
            _actionMap.Editor.X.performed += (c) =>
            {
                var node = _selectNodeController.GetSelectedNode();
                if(node != null)
                    DeleteNode(node);
            };
        }
        
        public void DeleteNode(Node node)
        {
            if(node.GetIsDeleted() == false) return;
            
            _nodeCreator.RemoveNode(node);
            // 1. Собираем ВСЕ уникальные связи ноды в один временный список
            List<NodeConnection> connectionsToRemove = new List<NodeConnection>();

            foreach (var port in node.inputPorts)
                connectionsToRemove.AddRange(port.Connections);

            foreach (var port in node.outputPorts)
                connectionsToRemove.AddRange(port.Connections);

            // 2. Удаляем каждую связь корректно
            // Используем обратный цикл или копию списка, так как Disconnect меняет оригинал
            for (int i = connectionsToRemove.Count - 1; i >= 0; i--)
            {
                if (connectionsToRemove[i] != null)
                    connectionsToRemove[i].Disconnect();
            }

            // 3. Очищаем логику и удаляем саму ноду
            node.Logic.ClearConnections();
            Destroy(node.gameObject);
        }
    }
}