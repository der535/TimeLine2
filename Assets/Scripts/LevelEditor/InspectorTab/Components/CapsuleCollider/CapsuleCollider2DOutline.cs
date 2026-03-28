using NaughtyAttributes;
using TimeLine;
using TimeLine.Installers;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using UnityEngine;
using Zenject;

public class CapsuleCollider2DOutline : MonoBehaviour
{
    [SerializeField] private Color color = Color.green;
    [SerializeField] private float pixelThickness = 1f;
    [Space]
    [SerializeField] private LineRenderer lineRenderer;
    // [SerializeField] private PhysicsAnchor physicsAnchor;
    [SerializeField] private CapsuleCollider2D capsuleCollider;
    [SerializeField] private Camera mainCamera;

    public CapsuleCollider2D CapsuleCollider => capsuleCollider;

    [Inject]
    private void Construct(MainObjects mainObjects)
    {
        mainCamera = mainObjects.MainCamera;
    }

    internal void SetActiveLineRenderer(bool active)
    {
        lineRenderer.enabled = active;
    }
    
    // internal void Setup(ActiveObjectControllerComponent activeObjectControllerComponent, TransformComponent transformComponent)
    // {
    //     physicsAnchor.Setup(activeObjectControllerComponent, transformComponent);
    // }

    [Button]
    internal void UpdateOutline()
    {
        if (mainCamera == null || capsuleCollider == null || lineRenderer == null || !lineRenderer.enabled)
            return;

        // Параметры капсулы
        CapsuleDirection2D direction = capsuleCollider.direction;
        Vector2 size = capsuleCollider.size;
        Vector2 center = capsuleCollider.offset;

        float radius = (direction == CapsuleDirection2D.Vertical) 
            ? size.x * 0.5f 
            : size.y * 0.5f;
        
        float centerPart = (direction == CapsuleDirection2D.Vertical) 
            ? size.y - size.x 
            : size.x - size.y;
        
        centerPart = Mathf.Max(0f, centerPart); // Защита от отрицательных значений

        // Учёт мирового масштаба
        Vector3 lossyScale = transform.lossyScale;
        float scaleX = Mathf.Abs(lossyScale.x);
        float scaleY = Mathf.Abs(lossyScale.y);

        float effectiveRadius = (direction == CapsuleDirection2D.Vertical) 
            ? radius * scaleX 
            : radius * scaleY;
        
        float effectiveCenterPart = (direction == CapsuleDirection2D.Vertical) 
            ? centerPart * scaleY 
            : centerPart * scaleX;

        // Мировые координаты центра
        Vector3 worldCenter = transform.TransformPoint(center);

        // Направления с учётом поворота объекта
        Vector3 localDirection = (direction == CapsuleDirection2D.Vertical) 
            ? Vector3.up 
            : Vector3.right;
        
        Vector3 worldDirection = transform.TransformDirection(localDirection).normalized;
        Vector3 worldPerpendicular = Vector3.Cross(worldDirection, Vector3.forward).normalized;

        // Позиции центров полукругов
        Vector3 topCenter = worldCenter + worldDirection * (effectiveCenterPart * 0.5f);
        Vector3 bottomCenter = worldCenter - worldDirection * (effectiveCenterPart * 0.5f);

        // Генерация точек контура
        const int arcSegments = 16;
        int totalPoints = (arcSegments + 1) * 2;
        Vector3[] positions = new Vector3[totalPoints];

        int index = 0;

        // Верхняя дуга (от левой точки к правой)
        for (int i = 0; i <= arcSegments; i++)
        {
            float t = i / (float)arcSegments;
            float angle = Mathf.PI * (1 - t); // π → 0 радиан
            Vector3 offset = Mathf.Cos(angle) * worldPerpendicular + Mathf.Sin(angle) * worldDirection;
            positions[index++] = topCenter + offset * effectiveRadius;
        }

        // Нижняя дуга (от правой точки к левой)
        for (int i = 0; i <= arcSegments; i++)
        {
            float t = i / (float)arcSegments;
            float angle = Mathf.PI * t; // 0 → π радиан
            Vector3 offset = Mathf.Cos(angle) * worldPerpendicular - Mathf.Sin(angle) * worldDirection;
            positions[index++] = bottomCenter + offset * effectiveRadius;
        }

        // Настройка LineRenderer
        lineRenderer.positionCount = totalPoints;
        lineRenderer.SetPositions(positions);
        lineRenderer.loop = true;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        // Расчёт толщины в мировых координатах
        float worldThickness = CalculatePixel.Calculate(pixelThickness, transform, mainCamera);
        lineRenderer.startWidth = worldThickness;
        lineRenderer.endWidth = worldThickness;
    }
}