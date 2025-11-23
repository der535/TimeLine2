using NaughtyAttributes;
using TimeLine.Installers;
using UnityEngine;
using Zenject;

public class BoxCollider2DOutline : MonoBehaviour
{
    [SerializeField] private Color color = Color.green;
    [SerializeField] private float pixelThickness = 1f; // Толщина в пикселях (по умолчанию 1)
    [Space]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private BoxCollider2D boxCollider;
    
    private Camera mainCamera;

    public BoxCollider2D BoxCollider => boxCollider;
    
    [Inject]
    private void Construct(MainObjects mainObjects)
    {
        mainCamera = mainObjects.MainCamera;
    }

    // private void Update()
    // {
    //     UpdateOutline();
    // }
    internal void SetActiveLineRenderer(bool active)
    {
        lineRenderer.enabled = active;
    }

    [Button]
    internal void UpdateOutline()
    {
        if (mainCamera == null || boxCollider == null || lineRenderer == null || lineRenderer.enabled == false)
            return;

        // Получаем размеры и смещение коллайдера в локальном пространстве объекта
        Vector2 center = boxCollider.offset;
        Vector2 size = boxCollider.size;

        // Определяем 4 угла прямоугольника в локальном пространстве (относительно центра коллайдера)
        Vector2[] localCorners = new Vector2[]
        {
            center + new Vector2( size.x / 2f,  size.y / 2f), // верхний правый
            center + new Vector2(-size.x / 2f,  size.y / 2f), // верхний левый
            center + new Vector2(-size.x / 2f, -size.y / 2f), // нижний левый
            center + new Vector2( size.x / 2f, -size.y / 2f)  // нижний правый
        };

        // Преобразуем углы в мировое пространство с учётом поворота и скейла
        Vector3[] worldCorners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            worldCorners[i] = transform.TransformPoint(localCorners[i]);
        }

        // Устанавливаем позиции в LineRenderer
        lineRenderer.positionCount = 4;
        lineRenderer.SetPositions(worldCorners);

        // Замыкаем контур
        lineRenderer.loop = true;

        // Цвет
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        // Толщина линии в мировых единицах
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