using System.Collections.Generic;
using Radishmouse;
using TimeLine.LevelEditor.ValueEditor;
using UnityEngine;
using Zenject;

public class NodeConnection : MonoBehaviour
{
    public RectTransform OutputPortRectTransform;
    public RectTransform InputPortRectTransform;
    
    public Port OutputPort;
    public Port InputPort;

    private RectTransform _rootLineRenderer;
    private Camera _mainCamera;
    private bool _isDragging; // Флаг: тянем ли мы линию сейчас

    [SerializeField] private UILineRenderer lineRenderer;
    [SerializeField] private NodeConnectionDetector nodeConnectionDetector;

    private NodeConnector _connector;

    [Inject]
    private void Constructor(NodeConnector connector)
    {
        _connector = connector;
    }
    
    
    public void Setup(RectTransform output, RectTransform input, RectTransform rootLineRenderer, Camera camera, Port outputPort, Port inputPort)
    {
        lineRenderer.raycastTarget = false;
        OutputPortRectTransform = output;
        InputPortRectTransform = input;
        OutputPort = outputPort;
        InputPort = inputPort;
        _rootLineRenderer = rootLineRenderer;
        _isDragging = false;
        _mainCamera = camera;
        nodeConnectionDetector.Setup(rootLineRenderer);
        this.OutputPort = outputPort;
        this.InputPort = inputPort;

        // Линия говорит портам: "Я теперь ваша"
        OutputPort.RegisterConnection(this);
        if (InputPort != null) 
            InputPort.RegisterConnection(this);
    }

    // Метод для начала создания связи (когда есть только выход, и тянем к мышке)
    public void Setup(RectTransform output, RectTransform rootLineRenderer, Camera camera, Port outputPort)
    {
        lineRenderer.raycastTarget = false;
        OutputPort = outputPort;
        OutputPortRectTransform = output;
        _rootLineRenderer = rootLineRenderer;
        InputPort = null; // Вход пока не задан
        _isDragging = true;
        _mainCamera = camera;
        nodeConnectionDetector.Setup(rootLineRenderer);
    }

    public void Disconnect()
    {
        // InputPort.Owner.Logic
        var index = InputPort.Owner.inputPorts.IndexOf(InputPort);
        InputPort.Owner.Logic.DisconnectInput(index);
        _connector.DeselectPort();
        _connector.RemoveConnection(this);
        InputPort.Owner.inputPorts[index].SetActiveDefaultInputValue(true);
        if (OutputPort != null) OutputPort.UnregisterConnection(this);
        if (InputPort != null) InputPort.UnregisterConnection(this);

        // Логика отключения данных (как мы писали раньше)
        if (InputPort != null)
        {
            InputPort.Owner.Logic.DisconnectInput(index);
            InputPort.SetActiveDefaultInputValue(true);
        }
        Destroy(this.gameObject);
    }
    
    private void Update()
    {
        // Нам как минимум нужен стартовый порт и контейнер
        if (!OutputPort || !_rootLineRenderer) return;
        
        DrawLine(_rootLineRenderer);
    }

    private void DrawLine(RectTransform rootLineRenderer)
    {
        // 1. Получаем мировую позицию начала (всегда от порта)
        Vector3 worldStart = OutputPortRectTransform.position;
        Vector3 worldEnd;

        // 2. Определяем мировую позицию конца
        if (_isDragging)
        {
            // Если тянем — берем позицию мыши (Input.mousePosition — это экранные координаты)
            // Но для корректности в мировом пространстве UI используем это:
            worldEnd = Input.mousePosition; 
        }
        else if (InputPort != null)
        {
            // Если связь установлена — берем позицию порта
            worldEnd = InputPortRectTransform.position;
        }
        else return;

        // 3. Конвертируем мировые/экранные точки в локальные координаты контейнера
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootLineRenderer, 
            RectTransformUtility.WorldToScreenPoint(null, worldStart), 
            null, 
            out Vector2 localStart
        );

        Vector2 localEnd;
        if (_isDragging)
        {
            // Для мыши конвертируем напрямую из экранных координат
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootLineRenderer, 
                worldEnd, 
                _mainCamera, 
                out localEnd
            );
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootLineRenderer, 
                RectTransformUtility.WorldToScreenPoint(null, worldEnd), 
                null, 
                out localEnd
            );
        }

        // 4. Отрисовка
        List<Vector2> positions = new List<Vector2>();
        
        // Для красоты можно добавить кривизну даже при перетаскивании
        float duration = Mathf.Abs(localStart.x - localEnd.x) * 0.5f;
        Vector2 control1 = localStart + Vector2.right * duration;
        Vector2 control2 = localEnd + Vector2.left * duration;
        

        // Если хочешь кривую Безье (рекомендуется):
        int segments = 20;
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            positions.Add(CalculateBezier(t, localStart, control1, control2, localEnd));
        }

        lineRenderer.SetPoints(positions.ToArray());
    }


    
    internal void SelectColor(bool isSelected)
    {
        lineRenderer.color = isSelected ? Color.blue : Color.white;
    }

    private Vector2 CalculateBezier(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float u = 1 - t;
        return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
    }
}