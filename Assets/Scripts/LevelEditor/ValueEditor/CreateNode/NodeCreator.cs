using System;
using System.Collections.Generic;
using NaughtyAttributes;
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

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        internal List<Node> GetNodes() => _nodes;

        [Button]
        private void CreateNode()
        {
            //Со старту создаёт 2 тестовые ноды
            OutputLogic outputLogic = new OutputLogic();
            outputLogic.Initialize(dataType);
            CreateNode(outputLogic, "Output", new Vector2(0, 0), false);
        }

        //Метод для вывода конечного значение
        [Button]
        private void GetValue()
        {
           print(_outputLogic.GetValue()); 
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
        internal Node CreateNode(NodeLogic nodeLogic, string nodeName, Vector2 position, bool isDeleted = true)
        {
            Node component = _container.InstantiatePrefab(_nodePrefab, root).GetComponent<Node>();
            RectTransform rect = (RectTransform)component.gameObject.transform;
            rect.anchoredPosition = position;
            component.Initialize(nodeLogic, nodeName, isDeleted);
            _nodes.Add(component);
            if (nodeLogic is OutputLogic outputLogic)
            {
                _outputLogic = outputLogic;
            }
            return component;
        }
    }
}