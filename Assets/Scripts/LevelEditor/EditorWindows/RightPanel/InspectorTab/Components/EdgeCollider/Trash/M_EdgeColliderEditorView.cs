// using System.Collections.Generic;
// using UnityEngine;
//
// namespace TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components.EdgeCollider
// {
//     public class M_EdgeColliderEditorView
//     {
//         private M_EdgeColliderEditorData _mEdgeColliderEditorData;
//         private GameObject _edgeColliderObject;
//
//         internal M_EdgeColliderEditorView(M_EdgeColliderEditorData mEdgeColliderEditorData, LineRenderer lineRenderer, GameObject EdgeColliderObject)
//         {
//             _mEdgeColliderEditorData = mEdgeColliderEditorData;
//             LineRenderer = lineRenderer;
//             _edgeColliderObject = EdgeColliderObject;
//         }
//
//         public List<LineRenderer> RadiusVisualizers = new();
//         public List<LineRenderer> EdgeConnectionVisualizers = new();
//
//         public Material redMaterial;
//         public Material circleMaterial;
//
//         public LineRenderer LineRenderer;
//
//         internal void UpdateWidth()
//         {
//             LineRenderer.startWidth = _mEdgeColliderEditorData.IntersectingSegmentWidthDynamic;
//             LineRenderer.endWidth = _mEdgeColliderEditorData.IntersectingSegmentWidthDynamic;
//
//             foreach (var VARIABLE in RadiusVisualizers)
//             {
//                 VARIABLE.startWidth = _mEdgeColliderEditorData.IntersectingSegmentWidthDynamic;
//                 VARIABLE.endWidth = _mEdgeColliderEditorData.IntersectingSegmentWidthDynamic;
//             }
//
//             foreach (var VARIABLE in EdgeConnectionVisualizers)
//             {
//                 VARIABLE.startWidth = _mEdgeColliderEditorData.IntersectingSegmentWidthDynamic;
//                 VARIABLE.endWidth = _mEdgeColliderEditorData.IntersectingSegmentWidthDynamic;
//             }
//         }
//         public void ReplaceAllPoints(List<Vector2> worldPoints)
//         {
//             if (worldPoints == null || worldPoints.Count == 0)
//             {
//                 // Если список пустой, создаем минимальный collider с двумя точками
//                 _mEdgeColliderEditorData.EdgeCollider2D.points = new Vector2[] { Vector2.zero, Vector2.right };
//             }
//             else
//             {
//                 _mEdgeColliderEditorData.EdgeCollider2D.points = worldPoints.ToArray();
//             }
//         
//             // Обновляем все визуальные элементы
//             UpdateOutline();
//             UpdateIntersectingSegments();
//             UpdateEdgeRadiusVisualization();
//             UpdateEdgeRadiusConnections();
//         
//             _mEdgeColliderEditorData.EdgeCollider2DComponent.BeginParameterUpdate();
//             // Обновляем параметр
//             
//         }
//         
//         internal void UpdateEdgeRadiusVisualization()
//         {
//             float radius = _mEdgeColliderEditorData.EdgeCollider2D.edgeRadius;
//             if (radius <= 0)
//             {
//                 foreach (var lr in RadiusVisualizers)
//                     lr.gameObject.SetActive(false);
//                 return;
//             }
//
//             int pointCount = _mEdgeColliderEditorData.EdgeCollider2D.points.Length;
//             while (RadiusVisualizers.Count < pointCount)
//             {
//                 var obj = new GameObject("RadiusCircle");
//                 obj.transform.SetParent(_edgeColliderObject.transform);
//                 var lr = obj.AddComponent<LineRenderer>();
//                 lr.material = circleMaterial;
//                 lr.startWidth = lr.endWidth = 0.05f;
//                 lr.positionCount = _mEdgeColliderEditorData.CircleResolution + 1;
//                 RadiusVisualizers.Add(lr);
//             }
//
//             Vector2[] worldPoints = new Vector2[pointCount];
//             for (int i = 0; i < pointCount; i++)
//                 worldPoints[i] =
//                     _edgeColliderObject.transform.TransformPoint(_mEdgeColliderEditorData.EdgeCollider2D.points[i]);
//
//             for (int i = 0; i < pointCount; i++)
//             {
//                 var lr = RadiusVisualizers[i];
//                 lr.gameObject.SetActive(true);
//                 Vector3[] positions = new Vector3[_mEdgeColliderEditorData.CircleResolution + 1];
//                 for (int j = 0; j <= _mEdgeColliderEditorData.CircleResolution; j++)
//                 {
//                     float angle = 2 * Mathf.PI * j / _mEdgeColliderEditorData.CircleResolution;
//                     positions[j] = worldPoints[i] + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
//                 }
//
//                 lr.SetPositions(positions);
//             }
//
//             for (int i = pointCount; i < RadiusVisualizers.Count; i++)
//                 RadiusVisualizers[i].gameObject.SetActive(false);
//         }
//
//         internal void UpdatePoint((int index, Vector2 worldPosition) data)
//         {
//             if (data.index < 0) return;
//             LineRenderer.SetPosition(data.index, data.worldPosition);
//         }
//
//         internal void UpdateEdgeRadiusConnections()
//         {
//             float radius = _mEdgeColliderEditorData.EdgeCollider2D.edgeRadius;
//             if (radius <= 0 || _mEdgeColliderEditorData.EdgeCollider2D.points.Length < 2)
//             {
//                 foreach (var lr in EdgeConnectionVisualizers)
//                     lr.gameObject.SetActive(false);
//                 return;
//             }
//
//             int segmentCount = _mEdgeColliderEditorData.EdgeCollider2D.points.Length - 1;
//             while (EdgeConnectionVisualizers.Count < segmentCount * 2)
//             {
//                 var obj = new GameObject("EdgeConnection");
//                 obj.transform.SetParent(_edgeColliderObject.transform);
//                 var lr = obj.AddComponent<LineRenderer>();
//                 lr.material = circleMaterial;
//                 lr.startWidth = lr.endWidth = radius * 2;
//                 lr.positionCount = 2;
//                 EdgeConnectionVisualizers.Add(lr);
//             }
//
//             Vector2[] worldPoints = new Vector2[_mEdgeColliderEditorData.EdgeCollider2D.points.Length];
//             for (int i = 0; i < _mEdgeColliderEditorData.EdgeCollider2D.points.Length; i++)
//                 worldPoints[i] =
//                     _edgeColliderObject.transform.TransformPoint(_mEdgeColliderEditorData.EdgeCollider2D.points[i]);
//
//             int connectionIndex = 0;
//             for (int i = 0; i < segmentCount; i++)
//             {
//                 Vector2 p1 = worldPoints[i];
//                 Vector2 p2 = worldPoints[i + 1];
//                 Vector2 segment = p2 - p1;
//                 Vector2 normal = new Vector2(-segment.y, segment.x).normalized;
//
//                 Vector3 leftStart = p1 + normal * radius;
//                 Vector3 leftEnd = p2 + normal * radius;
//                 Vector3 rightStart = p1 - normal * radius;
//                 Vector3 rightEnd = p2 - normal * radius;
//
//                 var leftLr = EdgeConnectionVisualizers[connectionIndex++];
//                 leftLr.gameObject.SetActive(true);
//                 leftLr.SetPosition(0, leftStart);
//                 leftLr.SetPosition(1, leftEnd);
//
//                 var rightLr = EdgeConnectionVisualizers[connectionIndex++];
//                 rightLr.gameObject.SetActive(true);
//                 rightLr.SetPosition(0, rightStart);
//                 rightLr.SetPosition(1, rightEnd);
//             }
//
//             for (int i = connectionIndex; i < EdgeConnectionVisualizers.Count; i++)
//                 EdgeConnectionVisualizers[i].gameObject.SetActive(false);
//         }
//
//         internal void UpdateIntersectingSegments()
//         {
//             ClearIntersectingSegments();
//             if (_mEdgeColliderEditorData.EdgeCollider2D.points.Length < 4) return;
//
//             Vector2[] worldPoints = new Vector2[_mEdgeColliderEditorData.EdgeCollider2D.points.Length];
//             for (int i = 0; i < _mEdgeColliderEditorData.EdgeCollider2D.points.Length; i++)
//                 worldPoints[i] =
//                     _edgeColliderObject.transform.TransformPoint(_mEdgeColliderEditorData.EdgeCollider2D.points[i]);
//
//             HashSet<int> intersectingSegments = new HashSet<int>();
//             for (int i = 0; i < worldPoints.Length - 1; i++)
//             {
//                 for (int j = i + 2; j < worldPoints.Length - 1; j++)
//                 {
//                     if (j == i + 1) continue;
//                     if (DoSegmentsIntersect(worldPoints[i], worldPoints[i + 1], worldPoints[j], worldPoints[j + 1]))
//                     {
//                         intersectingSegments.Add(i);
//                         intersectingSegments.Add(j);
//                     }
//                 }
//             }
//
//             foreach (int idx in intersectingSegments)
//                 CreateIntersectingSegment(worldPoints[idx], worldPoints[idx + 1]);
//         }
//
//         void CreateIntersectingSegment(Vector2 start, Vector2 end)
//         {
//             GameObject obj = GetSegmentObject();
//             LineRenderer lr = obj.GetComponent<LineRenderer>();
//             lr.positionCount = 2;
//             lr.SetPosition(0, start);
//             lr.SetPosition(1, end);
//             obj.SetActive(true);
//         }
//         void ClearIntersectingSegments()
//         {
//             foreach (var obj in _mEdgeColliderEditorData.IntersectingSegmentObjects)
//                 obj.SetActive(false);
//         }
//     
//
//
//         static bool OnSegment(Vector2 a, Vector2 b, Vector2 c) =>
//             c.x <= Mathf.Max(a.x, b.x) + M_EdgeColliderEditorBaseData.Epsilon &&
//             c.x >= Mathf.Min(a.x, b.x) - M_EdgeColliderEditorBaseData.Epsilon &&
//             c.y <= Mathf.Max(a.y, b.y) + M_EdgeColliderEditorBaseData.Epsilon &&
//             c.y >= Mathf.Min(a.y, b.y) - M_EdgeColliderEditorBaseData.Epsilon;
//
//         static float Orientation(Vector2 a, Vector2 b, Vector2 c) =>
//             (b.y - a.y) * (c.x - b.x) - (b.x - a.x) * (c.y - b.y);
//         static bool DoSegmentsIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
//         {
//             float o1 = Orientation(p1, q1, p2);
//             float o2 = Orientation(p1, q1, q2);
//             float o3 = Orientation(p2, q2, p1);
//             float o4 = Orientation(p2, q2, q1);
//
//             if (((o1 > M_EdgeColliderEditorBaseData.Epsilon && o2 < -M_EdgeColliderEditorBaseData.Epsilon) ||
//                  (o1 < -M_EdgeColliderEditorBaseData.Epsilon && o2 > M_EdgeColliderEditorBaseData.Epsilon)) &&
//                 ((o3 > M_EdgeColliderEditorBaseData.Epsilon && o4 < -M_EdgeColliderEditorBaseData.Epsilon) ||
//                  (o3 < -M_EdgeColliderEditorBaseData.Epsilon && o4 > M_EdgeColliderEditorBaseData.Epsilon)))
//                 return true;
//
//             if (Mathf.Abs(o1) < M_EdgeColliderEditorBaseData.Epsilon && OnSegment(p1, q1, p2)) return true;
//             if (Mathf.Abs(o2) < M_EdgeColliderEditorBaseData.Epsilon && OnSegment(p1, q1, q2)) return true;
//             if (Mathf.Abs(o3) < M_EdgeColliderEditorBaseData.Epsilon && OnSegment(p2, q2, p1)) return true;
//             if (Mathf.Abs(o4) < M_EdgeColliderEditorBaseData.Epsilon && OnSegment(p2, q2, q1)) return true;
//
//             return false;
//         }
//
//         GameObject GetSegmentObject()
//         {
//             foreach (var obj in _mEdgeColliderEditorData.IntersectingSegmentObjects)
//             {
//                 if (!obj.activeSelf)
//                     return obj;
//             }
//
//             GameObject newObj = new GameObject("IntersectingSegment");
//             newObj.transform.SetParent(_edgeColliderObject.transform);
//             var lr = newObj.AddComponent<LineRenderer>();
//             lr.material = redMaterial;
//             lr.startWidth = _mEdgeColliderEditorData.IntersectingSegmentWidthDynamic;
//             lr.endWidth = _mEdgeColliderEditorData.IntersectingSegmentWidthDynamic;
//             lr.numCornerVertices = 2;
//             _mEdgeColliderEditorData.IntersectingSegmentObjects.Add(newObj);
//             return newObj;
//         }
//
//         internal void Initialize()
//         {
//             redMaterial = new Material(Shader.Find("Sprites/Default")) { color = Color.red };
//             circleMaterial = new Material(Shader.Find("Sprites/Default"))
//                 { color = new Color(0, 1, 0, 0.5f) };
//         }
//
//         internal void SetActiveLineRenderer(bool active)
//         {
//             LineRenderer.enabled = active;
//         }
//
//         public void UpdateOutline()
//         {
//             Vector2[] points = _mEdgeColliderEditorData.EdgeCollider2D.points;
//             LineRenderer.positionCount = points.Length;
//             Debug.Log(LineRenderer);
//             Debug.Log(_edgeColliderObject);
//             for (int i = 0; i < points.Length; i++)
//             {
//                 Debug.Log(LineRenderer);
//                 Debug.Log(_edgeColliderObject);
//                 LineRenderer.SetPosition(i,
//                     _edgeColliderObject.transform.TransformPoint(points[i]));
//             }
//         }
//     }
// }