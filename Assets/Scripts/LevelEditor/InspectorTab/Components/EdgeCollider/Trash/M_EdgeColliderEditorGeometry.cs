// using System;
// using System.Collections.Generic;
// using System.Linq;
// using TimeLine.CustomInspector.Logic.Parameter;
// using TimeLine.Installers;
// using Unity.Mathematics;
// using UnityEngine;
//
// namespace TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components.EdgeCollider
// {
//     public class M_EdgeColliderEditorGeometry
//     {
//         private M_EdgeColliderEditorData _mEdgeColliderEditorData;
//         private GameObject _edgeColliderObject;
//         private GridScene _gridScene;
//         private M_EdgeColliderEditorMousePosition _mEdgeColliderEditorMousePosition;
//
//         internal M_EdgeColliderEditorGeometry(M_EdgeColliderEditorData mEdgeColliderEditorData,
//             M_EdgeColliderEditorMousePosition mEdgeColliderEditorMousePosition, GridScene gridScene,
//             GameObject edgeColliderObject)
//         {
//             _gridScene = gridScene;
//             _mEdgeColliderEditorMousePosition = mEdgeColliderEditorMousePosition;
//             _mEdgeColliderEditorData = mEdgeColliderEditorData;
//             _edgeColliderObject = edgeColliderObject;
//         }
//
//
//         internal void CalculateDynamicFields(CameraReferences cameraReferences)
//         {
//             _mEdgeColliderEditorData.DistanceToEdgeDynamic = M_EdgeColliderEditorBaseData.DistanceToEdgeBase *
//                                                              cameraReferences.editSceneCamera.orthographicSize;
//             _mEdgeColliderEditorData.DistanceToCornerDynamic = M_EdgeColliderEditorBaseData.DistanceToCornerBase *
//                                                                cameraReferences.editSceneCamera.orthographicSize;
//             _mEdgeColliderEditorData.IntersectingSegmentWidthDynamic =
//                 M_EdgeColliderEditorBaseData.IntersectingSegmentWidthBase *
//                 cameraReferences.editSceneCamera.orthographicSize;
//         }
//
//
//         internal void BeginDraggingPoint(int pointIndex) => _mEdgeColliderEditorData.DraggedPointIndex = pointIndex;
//
//
//         // void CreatePoint(Vector2 worldPosition)
//         // {
//         //     worldPosition = _gridScene.PositionFloatSnapToGrid(worldPosition, quaternion.identity);
//         //     Vector2 localPoint = transform.InverseTransformPoint(worldPosition);
//         //     Vector2[] oldPoints = EdgeCollider2D.points;
//         //     Vector2[] newPoints = new Vector2[oldPoints.Length + 1];
//         //     Array.Copy(oldPoints, newPoints, oldPoints.Length);
//         //     newPoints[oldPoints.Length] = localPoint;
//         //     EdgeCollider2D.points = newPoints;
//         //     UpdateOutline();
//         //     UpdateIntersectingSegments();
//         //     UpdateEdgeRadiusVisualization();
//         //     UpdateEdgeRadiusConnections();
//         //     EdgeCollider2DComponent.BeginParameterUpdate();
//         //
//         //     ListVector2Parameter.Value = EdgeCollider2D.points.ToList();
//         //     EdgeCollider2DComponent.EndParameterUpdate();
//         // }
//
//
//         void UpdatePositions(int index, Vector2 worldPosition)
//         {
//             worldPosition = _gridScene.PositionFloatSnapToGrid(worldPosition, quaternion.identity);
//             Vector2 localPosition = _edgeColliderObject.transform.InverseTransformPoint(worldPosition);
//             Vector2[] points = _mEdgeColliderEditorData.EdgeCollider2D.points;
//             points[index] = localPosition;
//             _mEdgeColliderEditorData.EdgeCollider2D.points = points;
// //update
//             SynchronizationListVector2Parameter();
//         }
//
//         internal void RemovePoint(int index)
//         {
//             if (_mEdgeColliderEditorData.EdgeCollider2D.points.Length <= 2) return;
//             Vector2[] oldPoints = _mEdgeColliderEditorData.EdgeCollider2D.points;
//             Vector2[] newPoints = new Vector2[oldPoints.Length - 1];
//             Array.Copy(oldPoints, 0, newPoints, 0, index);
//             Array.Copy(oldPoints, index + 1, newPoints, index, oldPoints.Length - index - 1);
//             _mEdgeColliderEditorData.EdgeCollider2D.points = newPoints;
// //update
//             SynchronizationListVector2Parameter();
//         }
//
//         internal int InsertPointBetween(int pointOne, int pointTwo, Vector2 worldPosition)
//         {
//             if (pointTwo != pointOne + 1)
//             {
//                 return -1;
//             }
//
//             Vector2 localPoint = _edgeColliderObject.transform.InverseTransformPoint(worldPosition);
//             Vector2[] oldPoints = _mEdgeColliderEditorData.EdgeCollider2D.points;
//             int insertIndex = pointTwo;
//             Vector2[] newPoints = new Vector2[oldPoints.Length + 1];
//             Array.Copy(oldPoints, 0, newPoints, 0, insertIndex);
//             newPoints[insertIndex] = localPoint;
//             Array.Copy(oldPoints, insertIndex, newPoints, insertIndex + 1, oldPoints.Length - insertIndex);
//             _mEdgeColliderEditorData.EdgeCollider2D.points = newPoints;
// //update
//             SynchronizationListVector2Parameter();
//             return insertIndex;
//         }
//
//         public void SynchronizationListVector2Parameter()
//         {
//             _mEdgeColliderEditorData.EdgeCollider2DComponent.BeginParameterUpdate();
//             _mEdgeColliderEditorData.ListVector2Parameter.Value =
//                 _mEdgeColliderEditorData.EdgeCollider2D.points.ToList();
//             _mEdgeColliderEditorData.EdgeCollider2DComponent.EndParameterUpdate();
//         }
//
//
//         Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
//         {
//             Vector2 ab = b - a;
//             float t = Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab);
//             t = Mathf.Clamp01(t);
//             return a + t * ab;
//         }
//
//         internal (int, int, float, Vector2) ClosestSegmentIndices(Vector2[] polyline, Vector2 point)
//         {
//             float minDistSqr = float.MaxValue;
//             int bestA = 0, bestB = 1;
//             Vector2 closestResult = Vector2.zero;
//             for (int i = 0; i < polyline.Length - 1; i++)
//             {
//                 Vector2 a = _edgeColliderObject.transform.TransformPoint(polyline[i]);
//                 Vector2 b = _edgeColliderObject.transform.TransformPoint(polyline[i + 1]);
//                 Vector2 closest = ClosestPointOnSegment(a, b, point);
//                 float distSqr = (closest - point).sqrMagnitude;
//                 if (distSqr < minDistSqr)
//                 {
//                     minDistSqr = distSqr;
//                     closestResult = closest;
//                     bestA = i;
//                     bestB = i + 1;
//                 }
//             }
//
//             return (bestA, bestB, minDistSqr, closestResult);
//         }
//
//
//         internal (int index, Vector2 worldPosition) UpdateDraggedPoint()
//         {
//             if (!_mEdgeColliderEditorData.DraggedPointIndex.HasValue) return (-1, Vector2.zero);
//
//             if (UnityEngine.Input.GetMouseButton(0))
//             {
//                 Vector3 mouseWorldPos = _mEdgeColliderEditorMousePosition.GetMouseWorldPosition();
//                 UpdatePositions(_mEdgeColliderEditorData.DraggedPointIndex.Value, mouseWorldPos);
//                 return (_mEdgeColliderEditorData.DraggedPointIndex.Value, mouseWorldPos);
//             }
//             else _mEdgeColliderEditorData.DraggedPointIndex = null;
//
//             return (-1, Vector2.zero);
//         }
//     }
// }