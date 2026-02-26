using System;
using NaughtyAttributes;
using TimeLine;
using TimeLine.Installers;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using UnityEngine;
using Zenject;

public class BoxCollider2DOutline : MonoBehaviour
{
    [SerializeField] private Color color = Color.green;
    [SerializeField] private float pixelThickness = 5f; // Толщина в пикселях (по умолчанию 1)
    [Space]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private PhysicsAnchor physicsAnchor;
    
    public BoxCollider2D BoxCollider => boxCollider;
    private CameraReferences _cameraReferences;
    
    [Inject]
    private void Construct(CameraReferences cameraReferences)
    {
        _cameraReferences = cameraReferences;
    }

    internal void Setup(ActiveObjectControllerComponent activeObjectControllerComponent, TransformComponent transformComponent)
    {
        physicsAnchor.Setup(activeObjectControllerComponent, transformComponent);
    }
    
    internal void SetActiveLineRenderer(bool active)
    {
        lineRenderer.enabled = active;
    }

    private void FixedUpdate()
    {
        // Толщина линии в мировых единицах
        float worldThickness = CalculatePixel.Calculate(pixelThickness, gameObject.transform, _cameraReferences.editSceneCamera);
        lineRenderer.startWidth = worldThickness;
        lineRenderer.endWidth = worldThickness;
    }

    [Button]
    internal void UpdateOutline()
    {
        if (_cameraReferences == null || boxCollider == null || lineRenderer == null || lineRenderer.enabled == false)
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


    }
}