using System.Collections.Generic;
using UnityEngine;
using TimeLine.Installers;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.General;
using Unity.Mathematics;

namespace TimeLine.EdgeColliderEditor
{
    public class EdgeColliderController
    {
        private readonly IEdgeColliderModel _model;
        private readonly EdgeColliderView _view;
        private readonly GridScene _gridScene;
        private readonly C_EdgeColliderSelectionCube _selectionCube;
        private readonly SceneToRawImageConverter _screenConverter;
        private readonly C_EditColliderState _editState;
        private readonly CameraReferences _cameraRefs;

        private int? _draggedPointIndex;
        private bool _isActive;

        public EdgeColliderController(
            IEdgeColliderModel model,
            EdgeColliderView view,
            GridScene gridScene,
            C_EdgeColliderSelectionCube selectionCube,
            SceneToRawImageConverter screenConverter,
            C_EditColliderState editState,
            CameraReferences cameraRefs)
        {
            _model = model;
            _view = view;
            _gridScene = gridScene;
            _selectionCube = selectionCube;
            _screenConverter = screenConverter;
            _editState = editState;
            _cameraRefs = cameraRefs;
        }

        public void Update()
        {
            if (_view.OutlineRenderer == null || !_view.OutlineRenderer.enabled || !_editState.GetState()) return;
            
            Vector2 mouseWorldPos = GetMouseWorldPosition();
            var points = _model.Points;
            var worldPoints = GetWorldPoints(points);

            // 1. Find closest vertex
            int closestVertexIndex = -1;
            float minVertexDistSqr = float.MaxValue;
            for (int i = 0; i < points.Count; i++)
            {
                float distSqr = Vector2.SqrMagnitude(worldPoints[i] - mouseWorldPos);
                if (distSqr < minVertexDistSqr)
                {
                    minVertexDistSqr = distSqr;
                    closestVertexIndex = i;
                }
            }

            // 2. Find closest edge
            var (segA, segB, minEdgeDistSqr, closestPointOnEdge) = ClosestSegmentIndices(worldPoints, mouseWorldPos);

            bool nearVertex = minVertexDistSqr <= _view.DistanceToCornerDynamic * _view.DistanceToCornerDynamic;
            bool nearEdge = minEdgeDistSqr <= _view.DistanceToEdgeDynamic * _view.DistanceToEdgeDynamic;

            // 3. Selection cube logic
            if (nearVertex)
            {
                _selectionCube.SetActive(true);
                _selectionCube.SetPosition(worldPoints[closestVertexIndex]);
            }
            else if (nearEdge)
            {
                _selectionCube.SetActive(true);
                _selectionCube.SetPosition(closestPointOnEdge);
            }
            else
            {
                _selectionCube.SetActive(false);
            }

            // 4. Input handling
            if (!_draggedPointIndex.HasValue && UnityEngine.Input.GetMouseButtonDown(0))
            {
                bool isCtrlPressed = UnityEngine.Input.GetKey(KeyCode.LeftControl) ||
                                     UnityEngine.Input.GetKey(KeyCode.RightControl);

                if (nearVertex)
                {
                    if (isCtrlPressed && points.Count > 2) RemovePoint(closestVertexIndex);
                    else BeginDraggingPoint(closestVertexIndex);
                }
                else if (nearEdge)
                {
                    int newIndex = InsertPointBetween(segA, segB, closestPointOnEdge);
                    if (newIndex >= 0) BeginDraggingPoint(newIndex);
                }
            }

            // 5. Dragging & visual updates
            UpdateDraggedPoint(mouseWorldPos);
            _view.UpdateIntersectingSegments(worldPoints);
            _view.UpdateEdgeRadiusVisualization(worldPoints, _model.EdgeRadius);
            _view.UpdateEdgeRadiusConnections(worldPoints, _model.EdgeRadius);
        }

        // --- Core Actions ---
        private void BeginDraggingPoint(int pointIndex) => _draggedPointIndex = pointIndex;

        private void UpdateDraggedPoint(Vector2 mouseWorldPos)
        {
            if (!_draggedPointIndex.HasValue) return;

            if (UnityEngine.Input.GetMouseButton(0) && _draggedPointIndex.Value < _model.Points.Count)
            {
                var snappedPos = _gridScene.PositionFloatSnapToGrid(mouseWorldPos, quaternion.identity);
                UpdatePointPosition(_draggedPointIndex.Value, snappedPos);
            }
            else
            {
                _draggedPointIndex = null;
            }
        }

        private void UpdatePointPosition(int index, Vector2 worldPosition)
        {
            var points = _model.Points;
            Vector2 localPosition = _view.ColliderTransform.InverseTransformPoint(worldPosition);
            points[index] = localPosition;
            ApplyPoints(points);
        }

        private void RemovePoint(int index)
        {
            var points = _model.Points;
            points.RemoveAt(index);
            ApplyPoints(points);
        }
        
        private int InsertPointBetween(int pointOne, int pointTwo, Vector2 worldPosition)
        {
            if (pointTwo != pointOne + 1) return -1;

            var points = _model.Points;
            Vector2 localPoint = _view.ColliderTransform.InverseTransformPoint(worldPosition);
            points.Insert(pointTwo, localPoint);
            ApplyPoints(points);
            return pointTwo;
        }

        internal void ApplyPoints(List<Vector2> points)
        {
            _model.BeginUpdate();
            _model.Points = points;
            _model.EndUpdate();
            _view.UpdateOutline(points);
        }

        // --- Geometry Helpers ---
        private List<Vector2> GetWorldPoints(List<Vector2> localPoints)
        {
            var worldPoints = new List<Vector2>(localPoints.Count);
            foreach (var point in localPoints)
                worldPoints.Add(_view.ColliderTransform.TransformPoint(point));
            return worldPoints;
        }

        private Vector2 GetMouseWorldPosition()
        {
            return _screenConverter.ScreenPointToWorldScene(UnityEngine.Input.mousePosition);
        }

        private Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ab = b - a;
            float t = Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab);
            t = Mathf.Clamp01(t);
            return a + t * ab;
        }

        private (int segA, int segB, float minDistSqr, Vector2 closestPoint) 
            ClosestSegmentIndices(List<Vector2> worldPoints, Vector2 point)
        {
            float minDistSqr = float.MaxValue;
            int bestA = 0, bestB = 1;
            Vector2 closestResult = Vector2.zero;

            for (int i = 0; i < worldPoints.Count - 1; i++)
            {
                Vector2 a = worldPoints[i];
                Vector2 b = worldPoints[i + 1];
                Vector2 closest = ClosestPointOnSegment(a, b, point);
                float distSqr = (closest - point).sqrMagnitude;
                if (distSqr < minDistSqr)
                {
                    minDistSqr = distSqr;
                    closestResult = closest;
                    bestA = i;
                    bestB = i + 1;
                }
            }

            return (bestA, bestB, minDistSqr, closestResult);
        }
    }
}