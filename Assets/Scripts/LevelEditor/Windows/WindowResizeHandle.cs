using UnityEngine;
using UnityEngine.EventSystems;

public enum ResizeDirection
{
    Left,
    Right,
    Top,
    Bottom,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public class ResizeHandle : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    [SerializeField] private ResizeDirection direction;
    [SerializeField] private RectTransform targetWindow;
    [SerializeField] private Vector2 minSize = new Vector2(100, 100);
    [SerializeField] private Vector2 maxSize = new Vector2(800, 600);
    
    private RectTransform targetRect;
    private Vector2 originalSize;
    private Vector2 originalMousePos;
    private Canvas canvas;

    private void Awake()
    {
        if (targetWindow == null)
            targetWindow = transform.parent.GetComponent<RectTransform>();
        
        targetRect = targetWindow;
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalSize = targetRect.sizeDelta;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            eventData.position, 
            canvas.worldCamera, 
            out originalMousePos
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Получаем текущую позицию мыши
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            eventData.position, 
            canvas.worldCamera, 
            out Vector2 currentMousePos
        );
        
        // Вычисляем дельту
        Vector2 delta = currentMousePos - originalMousePos;
        
        // Инвертируем для левых и нижних сторон
        float deltaX = delta.x;
        float deltaY = delta.y;
        
        if (direction == ResizeDirection.Left || 
            direction == ResizeDirection.TopLeft || 
            direction == ResizeDirection.BottomLeft)
            deltaX = -deltaX;
            
        if (direction == ResizeDirection.Bottom || 
            direction == ResizeDirection.BottomLeft || 
            direction == ResizeDirection.BottomRight)
            deltaY = -deltaY;
        
        Vector2 newSize = originalSize;
        
        // Применяем изменения в зависимости от направления
        if (direction == ResizeDirection.Left || direction == ResizeDirection.Right ||
            direction == ResizeDirection.TopLeft || direction == ResizeDirection.BottomLeft ||
            direction == ResizeDirection.TopRight || direction == ResizeDirection.BottomRight)
            newSize.x = Mathf.Clamp(originalSize.x + deltaX, minSize.x, maxSize.x);
            
        if (direction == ResizeDirection.Top || direction == ResizeDirection.Bottom ||
            direction == ResizeDirection.TopLeft || direction == ResizeDirection.TopRight ||
            direction == ResizeDirection.BottomLeft || direction == ResizeDirection.BottomRight)
            newSize.y = Mathf.Clamp(originalSize.y + deltaY, minSize.y, maxSize.y);
        
        // Сохраняем пивот окна
        Vector2 pivot = targetRect.pivot;
        Vector2 anchoredPos = targetRect.anchoredPosition;
        
        // Корректируем позицию при изменении размера
        if (direction == ResizeDirection.Left || 
            direction == ResizeDirection.TopLeft || 
            direction == ResizeDirection.BottomLeft )
        {
            float widthDiff = newSize.x - targetRect.sizeDelta.x;
            anchoredPos.x -= widthDiff * (1 - pivot.x);
        }
        
        if (direction == ResizeDirection.Bottom || 
            direction == ResizeDirection.BottomLeft || 
            direction == ResizeDirection.BottomRight)
        {
            float heightDiff = newSize.y - targetRect.sizeDelta.y;
            anchoredPos.y -= heightDiff * (1 - pivot.y);
        }

        if (direction == ResizeDirection.Right ||
            direction == ResizeDirection.BottomRight ||
            direction == ResizeDirection.TopRight)
        {
            float widthDiff = newSize.x - targetRect.sizeDelta.x;
            anchoredPos.x += widthDiff * (1 - pivot.x);
        }

        if (direction == ResizeDirection.Top ||
            direction == ResizeDirection.TopLeft ||
            direction == ResizeDirection.TopRight)
        {
            float heightDiff = newSize.y - targetRect.sizeDelta.y;
            anchoredPos.y += heightDiff * (1 - pivot.y);
        }

        // Применяем
        targetRect.sizeDelta = newSize;
        targetRect.anchoredPosition = anchoredPos;
    }
}