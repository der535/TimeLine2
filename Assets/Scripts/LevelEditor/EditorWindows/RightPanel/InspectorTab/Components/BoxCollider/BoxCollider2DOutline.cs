using NaughtyAttributes;
using TimeLine;
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
        float worldThickness = CalculatePixel.Calculate(pixelThickness, gameObject.transform, mainCamera);
        lineRenderer.startWidth = worldThickness;
        lineRenderer.endWidth = worldThickness;
    }
}