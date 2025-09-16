using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class BoxCollider2DOutline : MonoBehaviour
{
    [SerializeField] private Color color = Color.green;
    [SerializeField] private float pixelThickness = 1f; // Толщина в пикселях (по умолчанию 1)
    [SerializeField] private Camera mainCamera;
    [Space]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private BoxCollider2D boxCollider;
    

    private void Update()
    {
        UpdateOutline();
    }

    [Button]
    private void UpdateOutline()
    {
        if (mainCamera == null) return;

        // Получаем размеры и центр коллайдера
        Vector2 center = boxCollider.offset;
        Vector2 size = boxCollider.size;

        // Устанавливаем позиции углов (относительно центра коллайдера)
        lineRenderer.positionCount = 4;
        lineRenderer.SetPosition(0, center + new Vector2(size.x / 2, size.y / 2) + (Vector2)gameObject.transform.position);
        lineRenderer.SetPosition(1, center + new Vector2(-size.x / 2, size.y / 2) + (Vector2)gameObject.transform.position);
        lineRenderer.SetPosition(2, center + new Vector2(-size.x / 2, -size.y / 2) + (Vector2)gameObject.transform.position);
        lineRenderer.SetPosition(3, center + new Vector2(size.x / 2, -size.y / 2) + (Vector2)gameObject.transform.position);

        // Замыкаем контур (опционально, если хотите замкнутый прямоугольник)
        lineRenderer.loop = true;

        // Устанавливаем цвет
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        // Рассчитываем толщину в мировых единицах, соответствующую 1 пикселю на экране
        float worldThickness = CalculateWorldThicknessFromPixels(pixelThickness);
        lineRenderer.startWidth = worldThickness;
        lineRenderer.endWidth = worldThickness;
    }

    /// <summary>
    /// Рассчитывает толщину в мировых единицах, соответствующую заданному количеству пикселей на экране.
    /// </summary>
    private float CalculateWorldThicknessFromPixels(float pixelCount)
    {
        // Получаем расстояние от камеры до объекта (для ортографической камеры это не важно, но для перспективы — критично)
        float distance = Vector3.Distance(transform.position, mainCamera.transform.position);

        // Для ортографической камеры:
        if (mainCamera.orthographic)
        {
            // В ортографической камере 1 юнит = orthographicSize * 2 / Screen.height пикселей по вертикали
            float pixelsPerUnit = Screen.height / (mainCamera.orthographicSize * 2f);
            return pixelCount / pixelsPerUnit;
        }
        else
        {
            // Для перспективной камеры используем ScreenPointToRay и расстояние
            Vector3 upper = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f + pixelCount / (2f * Screen.height), distance));
            Vector3 lower = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f - pixelCount / (2f * Screen.height), distance));
            return Vector3.Distance(upper, lower);
        }
    }
}