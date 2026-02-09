using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor
{
    public class Port : MonoBehaviour
    {
        [SerializeField] private bool isInput;
        [SerializeField] private TextMeshProUGUI textMeshProUGUI;

        [SerializeField] private GameObject defaultInputValueConnector;
        [SerializeField] private RectTransform defaultInputValueRoot;
        [SerializeField] private SelectNode selectNode;

        private NodeConnector _connector;

        public Node Owner { get; private set; }
        public DataType type { get; private set; }
        public RectTransform PortTransform;
        
        public List<NodeConnection> Connections = new List<NodeConnection>();
// Помощник для быстрой проверки
        public bool HasConnections => Connections.Count > 0;

        public void RegisterConnection(NodeConnection connection)
        {
            if (!Connections.Contains(connection))
                Connections.Add(connection);
        }

        public void UnregisterConnection(NodeConnection connection)
        {
            if (Connections.Contains(connection))
                Connections.Remove(connection);
        }
        
        [Inject]
        private void Constructor(NodeConnector connector)
        {
            _connector = connector;
        }

        public SelectNode GetSelectNode() => selectNode; 

        public bool GetIsInput() => isInput;

        public void Setup(DataType dataType, Node node, string label)
        {
            type = dataType;
            Owner = node;
            textMeshProUGUI.text = label;
        }

        /// <summary>
        /// Выдаёт линию соединения ручного ввода с портом
        /// </summary>
        /// <returns></returns>
        public RectTransform GetInputValueRoot() => defaultInputValueRoot;

        /// <summary>
        /// Включает или выключает ручной ввод своего значения
        /// </summary>
        /// <param name="value"></param>
        public void SetActiveDefaultInputValue(bool value) => defaultInputValueConnector.gameObject.SetActive(value);

        public void OnPortClick()
        {
            _connector.OnPortClick(this);
        }

        public void OnPortSelect()
        {
            _connector.OnPortSelect(this);
        }

        public void OnPortDeselect()
        {
            _connector.DeselectPort();
        }
    }
}