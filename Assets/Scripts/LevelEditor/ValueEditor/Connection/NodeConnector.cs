using System.Collections.Generic;
using System.Linq;
using TimeLine.LevelEditor.ValueEditor;
using UnityEngine;
using Zenject;

public class NodeConnector : MonoBehaviour
{
    [SerializeField] private NodeConnection connectionPrefab;
    [SerializeField] private RectTransform linesRoot;
    [SerializeField] private Camera _mainCamera;

    private Port _startPort;
    private Port _endPort;

    private NodeConnection _currentLine;
    private List<NodeConnection> _allConnections = new();
    private DiContainer _diContainer;

    [Inject]
    private void Constructor(DiContainer diContainer)
    {
        _diContainer = diContainer;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            CheckConnection();
        }
    }

    public void OnPortClick(Port clickedPort)
    {
        // Нельзя начинать со входа (обычно тянем от выхода)
        if (clickedPort.GetIsInput()) return;
        _currentLine = _diContainer.InstantiatePrefab(connectionPrefab, linesRoot).GetComponent<NodeConnection>();

        _startPort = clickedPort;
        _currentLine.Setup(_startPort.PortTransform, linesRoot, _mainCamera, _startPort);
    }

    public void OnPortSelect(Port clickedPort)
    {
        _endPort = clickedPort;
    }

    public void DeselectPort()
    {
        _endPort = null;
    }

    public void RemoveConnection(NodeConnection connection)
    {
        _allConnections.Remove(connection);
    }

    public void CheckConnection()
    {
        if (_endPort && _startPort && _endPort.GetIsInput() && _endPort.type == _startPort.type &&
            _endPort.Owner != _startPort.Owner)
        {
            // Находим индекс входа, в который хотим "воткнуться"
            int inIdx = _endPort.Owner.inputPorts.IndexOf(_endPort);
            int outIdx = _startPort.Owner.outputPorts.IndexOf(_startPort);

            // ПРОВЕРКА НА ДУБЛИКАТ
            // Проверяем: есть ли уже в этом входе связь с этой же нодой и этим же выходом?
            if (_endPort.Owner.Logic.ConnectedInputs.TryGetValue(inIdx, out var existingConnection))
            {
                if (existingConnection.node == _startPort.Owner.Logic && existingConnection.outputIndex == outIdx)
                {
                    Debug.LogWarning("Такая связь уже существует!");
                    CancelConnection();
                    return;
                }
            }

            foreach (var VARIABLE in _allConnections.ToList())
            {
                print(VARIABLE);
                if (VARIABLE.InputPort != null && VARIABLE.InputPort == _endPort)
                {
                    Debug.LogWarning("Связь существует уже с другой нодой");


                    _endPort.Owner.Logic.DisconnectInput(inIdx);
                    _allConnections.Remove(VARIABLE);
                    Destroy(VARIABLE.gameObject);


                    break;
                }
            }

            CompleteConnection(_endPort);
        }
        else
        {
            CancelConnection();
        }
    }
    
    internal void RestoreConnections(List<ConnectionSaveEntry> connections, Dictionary<string, Node> idToNode)
    {
        foreach (var cData in connections)
        {
            // Находим визуальные ноды по ID
            if (idToNode.TryGetValue(cData.OutNodeId, out Node outNode) &&
                idToNode.TryGetValue(cData.InNodeId, out Node inNode))
            {
                // Получаем конкретные порты по индексам
                Port outPort = outNode.outputPorts[cData.OutIndex];
                Port inPort = inNode.inputPorts[cData.InIndex];

                // 1. Логическая связь
                inNode.Logic.ConnectInput(cData.InIndex, outNode.Logic, cData.OutIndex);

                // 2. Визуальная связь (создаем объект NodeConnection)
                // Вызываем твой метод создания линии, который мы обсуждали раньше
                CreateConnection(outPort, inPort);
            
                // 3. Выключаем поле ввода у входного порта, так как теперь там провод
                inPort.SetActiveDefaultInputValue(false);
            }
        }
    }

    private void CancelConnection()
    {
        _startPort = null;
        _endPort = null;
        if (_currentLine != null) Destroy(_currentLine.gameObject);
        _currentLine = null;
    }

    private void CompleteConnection(Port endPort)
    {
        // print("CompleteConnection");

        _currentLine.Setup(_startPort.PortTransform, endPort.PortTransform, linesRoot, _mainCamera, _startPort,
            endPort);

        _allConnections.Add(_currentLine);

        // СВЯЗЫВАЕМ ЛОГИКУ
        // Берём порт -> к какой ноде относится -> входящие порты у этой ноды -> получаем индекс этого порта из списка портов
        int inIdx = endPort.Owner.inputPorts.IndexOf(endPort);
        int outIdx = _startPort.Owner.outputPorts.IndexOf(_startPort);

        //Отключаем дефолтный ввод значение у входящего порта
        endPort.SetActiveDefaultInputValue(false);


        endPort.Owner.Logic.ConnectInput(inIdx, _startPort.Owner.Logic, outIdx);

        
        
        _startPort = null;
        _currentLine = null;
    }
    
    private void CreateConnection(Port start, Port endPort)
    {
        // print("CompleteConnection");

        NodeConnection connect = _diContainer.InstantiatePrefab(connectionPrefab, linesRoot).GetComponent<NodeConnection>();
        
        
        connect.Setup(start.PortTransform, endPort.PortTransform, linesRoot, _mainCamera, start,
            endPort);

        _allConnections.Add(connect);

        // СВЯЗЫВАЕМ ЛОГИКУ
        // Берём порт -> к какой ноде относится -> входящие порты у этой ноды -> получаем индекс этого порта из списка портов
        int inIdx = endPort.Owner.inputPorts.IndexOf(endPort);
        int outIdx = start.Owner.outputPorts.IndexOf(start);

        //Отключаем дефолтный ввод значение у входящего порта
        endPort.SetActiveDefaultInputValue(false);


        endPort.Owner.Logic.ConnectInput(inIdx, start.Owner.Logic, outIdx);
    }
}