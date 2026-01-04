using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDragger : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    [SerializeField] private RectTransform windowTransform;
    private RectTransform _canvasRectTransform;
    private Vector2 _pointerOffset;

    void Awake()
    {
        if (windowTransform == null)
            windowTransform = GetComponent<RectTransform>();

        Canvas canvas = windowTransform.GetComponentInParent<Canvas>();
        if (canvas != null)
            _canvasRectTransform = canvas.transform as RectTransform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Запоминаем смещение курсора относительно центра окна в экранных координатах
        RectTransformUtility.ScreenPointToLocalPointInRectangle(windowTransform, eventData.position, eventData.pressEventCamera, out _pointerOffset);
        
        // Выводим окно на передний план при клике
        windowTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (windowTransform == null || _canvasRectTransform == null) return;

        Vector2 currentMousePosition = eventData.position;
        Vector3[] canvasCorners = new Vector3[4];
        _canvasRectTransform.GetWorldCorners(canvasCorners);

        // 1. Рассчитываем новую желаемую позицию в мировых координатах
        Vector3 targetWorldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(windowTransform, currentMousePosition, eventData.pressEventCamera, out targetWorldPos);

        // 2. Применяем смещение, чтобы курсор не прыгал в центр
        // (Переводим локальное смещение в мировое пространство)
        Vector3 worldOffset = windowTransform.TransformVector(_pointerOffset);
        targetWorldPos -= worldOffset;

        // 3. Ограничиваем позицию с учетом размеров окна
        windowTransform.position = targetWorldPos; // Временно ставим, чтобы GetWorldCorners сработал
        windowTransform.position = ClampToCanvas(canvasCorners);
    }

    private Vector3 ClampToCanvas(Vector3[] canvasCorners)
    {
        Vector3[] windowCorners = new Vector3[4];
        windowTransform.GetWorldCorners(windowCorners);

        Vector3 currentPos = windowTransform.position;

        // Мировые границы Canvas
        float minCanvasX = canvasCorners[0].x;
        float maxCanvasX = canvasCorners[2].x;
        float minCanvasY = canvasCorners[0].y;
        float maxCanvasY = canvasCorners[2].y;

        // Мировые границы Окна
        float windowWidth = windowCorners[2].x - windowCorners[0].x;
        float windowHeight = windowCorners[2].y - windowCorners[0].y;

        // Корректировка X
        if (windowCorners[0].x < minCanvasX) 
            currentPos.x += minCanvasX - windowCorners[0].x;
        if (windowCorners[2].x > maxCanvasX) 
            currentPos.x -= windowCorners[2].x - maxCanvasX;

        // Корректировка Y
        if (windowCorners[0].y < minCanvasY) 
            currentPos.y += minCanvasY - windowCorners[0].y;
        if (windowCorners[2].y > maxCanvasY) 
            currentPos.y -= windowCorners[2].y - maxCanvasY;

        return currentPos;
    }
}