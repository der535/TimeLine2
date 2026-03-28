// using NaughtyAttributes;
// using TimeLine;
// using TimeLine.Installers;
// using TimeLine.LevelEditor.Core;
// using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
// using UnityEngine;
// using Zenject;
//
// public class CircleCollider2DOutline : MonoBehaviour
// {
//     [SerializeField] private Color color = Color.green;
//     [Space]
//     [SerializeField] private LineRenderer lineRenderer;
//     [SerializeField] private PhysicsAnchor physicsAnchor;
//     [SerializeField] private CircleCollider2D circleCollider;
//
//
//     private CameraReferences _cameraReferences;
//     
//     public CircleCollider2D CircleCollider => circleCollider;
//
//     [Inject]
//     private void Construct(CameraReferences cameraReferences)
//     {
//         _cameraReferences = cameraReferences;
//     }
//
//     internal void SetActiveLineRenderer(bool active)
//     {
//         lineRenderer.enabled = active;
//     }
//     
//     internal void Setup(ActiveObjectControllerComponent activeObjectControllerComponent, TransformComponent transformComponent)
//     {
//         physicsAnchor.Setup(activeObjectControllerComponent, transformComponent);
//     }
//     
//     private void FixedUpdate()
//     {
//         // Толщина линии в мировых единицах
//         float worldThickness = CalculatePixel.Calculate(5, gameObject.transform, _cameraReferences.editSceneCamera);
//         lineRenderer.startWidth = worldThickness;
//         lineRenderer.endWidth = worldThickness;
//     }
//
//     [Button]
//     internal void UpdateOutline()
//     {
//         if (circleCollider == null || lineRenderer == null || !lineRenderer.enabled)
//             return;
//
//         Vector2 center = circleCollider.offset;
//         float radius = circleCollider.radius;
//
//         // Получаем мировой масштаб (lossyScale учитывает всю иерархию)
//         Vector3 lossyScale = transform.lossyScale;
//         float effectiveRadius = radius * Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));
//
//         // Мировая позиция центра коллайдера
//         Vector3 worldCenter = transform.TransformPoint(center);
//
//         // Ориентация (без scale)
//         Quaternion rotation = transform.rotation;
//
//         int segments = 32;
//         Vector3[] positions = new Vector3[segments + 1];
//
//         for (int i = 0; i <= segments; i++)
//         {
//             float angle = (2f * Mathf.PI / segments) * i;
//             Vector3 localOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
//             Vector3 worldOffset = rotation * localOffset * effectiveRadius;
//             positions[i] = worldCenter + worldOffset;
//         }
//
//         lineRenderer.sortingOrder = 1000001;
//         lineRenderer.positionCount = segments + 1;
//         lineRenderer.SetPositions(positions);
//         lineRenderer.loop = true;
//         lineRenderer.startColor = color;
//         lineRenderer.endColor = color;
//     }
// }