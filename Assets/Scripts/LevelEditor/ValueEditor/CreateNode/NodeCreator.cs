using System;
using System.Collections.Generic;
using EventBus;
using NaughtyAttributes;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.ValueEditor.NodeLogic;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor
{
    /// <summary>
    /// Самый главный создаёт ноды
    /// </summary>
    public class NodeCreator : MonoBehaviour
    {
        [SerializeField] private Node _nodePrefab;
        [SerializeField] private RectTransform root;
        [SerializeField] private DataType dataType;

        private DiContainer _container;

        private List<Node> _nodes = new();
        private OutputLogic _outputLogic; //OutputLogic Что бы получать конечное значение
        private GameEventBus _gameEventBus;

        private List<IInitializedNode> _initializedNodes = new();
        
        [Inject]
        private void Construct(DiContainer container, GameEventBus eventBus)
        {
            _container = container;
            _gameEventBus = eventBus;
        }

        internal List<Node> GetNodes() => _nodes;
        internal List<IInitializedNode> GetInitializedNodes() => _initializedNodes;
        
        
        public OutputLogic CreateNode(OutputLogic outputLogic)
        {
           return (OutputLogic)CreateNode(outputLogic, outputLogic.GetType().ToString(),
                Vector2.zero, false).Logic;
        }
        
        public void SetListIInitializedNodes(List<IInitializedNode> initializedNodes)
        {
            _initializedNodes = initializedNodes;
        }

        internal void RemoveNode(Node node)
        {
            _nodes.Remove(node);
        }

        /// <summary>
        /// Создание ноды
        /// </summary>
        /// <param name="nodeLogic">Класс логики ноды</param>
        /// <param name="nodeName">Название ноды</param>
        /// <param name="position">Начальная позиция</param>
        internal Node CreateNode(global::NodeLogic nodeLogic, string nodeName, Vector2 position, bool isDeleted = true)
        {
            _container.Inject(nodeLogic);
            Node component = _container.InstantiatePrefab(_nodePrefab, root).GetComponent<Node>();
            RectTransform rect = (RectTransform)component.gameObject.transform;
            rect.anchoredPosition = position;
            component.Initialize(nodeLogic, nodeName, isDeleted);
            _nodes.Add(component);
            if (nodeLogic is OutputLogic outputLogic)
            {
                _outputLogic = outputLogic;
            }
            
            if(nodeLogic is IInitializedNode initializedNode)
                _initializedNodes.Add(initializedNode);

            return component;
        }
    }
}