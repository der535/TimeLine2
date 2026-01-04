using UnityEngine;

public class RadialFillRotation : MonoBehaviour
{
    private RectTransform rectTransform;
    private Canvas canvas;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>(); // Для корректного преобразования координат
    }
    

    public void RotateTowardsCursor()
    {
        // Получаем позицию курсора в экранных координатах
        Vector2 cursorScreenPosition = Input.mousePosition;

        // Преобразуем экранные координаты в локальные координаты внутри Canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            cursorScreenPosition,
            canvas.worldCamera,
            out Vector2 localCursorPosition
        );

        // Вычисляем направление от центра объекта к курсору
        Vector2 direction = localCursorPosition - rectTransform.anchoredPosition;
        direction.Normalize();

        // Рассчитываем угол между осью "вправо" и направлением
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Применяем поворот (учитываем смещение радиального заполнения)
        rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}