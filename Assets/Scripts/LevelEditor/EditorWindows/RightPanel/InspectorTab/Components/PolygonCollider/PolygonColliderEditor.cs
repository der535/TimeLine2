using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class PolygonColliderEditor : MonoBehaviour
    {
        [FormerlySerializedAs("edgeCollider2D")] [SerializeField] PolygonCollider2D polygonCollider2D;
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] GameObject selectionCube;
        [SerializeField] float distanceToEdge = 0.5f;
        [SerializeField] float distanceToCorner = 0.3f;
        [SerializeField] float intersectingSegmentWidth = 0.1f;
        int? draggedPointIndex = null;
        List<GameObject> intersectingSegmentObjects = new List<GameObject>();
        Material redMaterial;
        Material circleMaterial;
        const float Epsilon = 0.0001f;
        const int circleResolution = 20;
        GridScene _gridScene;

        [Inject]
        void Construct(GridScene gridScene) => _gridScene = gridScene;

        [Button]
        void Start()
        {
            Initialize();
            Reset();
            CreatePoint(new Vector2(1, 2));
            CreatePoint(new Vector2(3, 4));
            CreatePoint(new Vector2(2, 1));
            UpdateIntersectingSegments();
        }

        void Initialize()
        {
            redMaterial = new Material(Shader.Find("Sprites/Default")) { color = Color.red };
            circleMaterial = new Material(Shader.Find("Sprites/Default")) { color = new Color(0, 1, 0, 0.5f) };
        }

        void Reset()
        {
            polygonCollider2D.points = new []{new Vector2(0,0),new Vector2(1,0),new Vector2(0.5f, 1)};
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.TransformPoint(polygonCollider2D.points[0]));
            lineRenderer.SetPosition(1, transform.TransformPoint(polygonCollider2D.points[1]));
            UpdateIntersectingSegments();
        }

        void UpdatePositions(int index, Vector2 worldPosition)
        {
            worldPosition = _gridScene.PositionFloatSnapToGrid(worldPosition, quaternion.identity);
            Vector2 localPosition = transform.InverseTransformPoint(worldPosition);
            Vector2[] points = polygonCollider2D.points;
            points[index] = localPosition;
            polygonCollider2D.points = points;
            lineRenderer.SetPosition(index, worldPosition);
            UpdateIntersectingSegments();
        }

        void CreatePoint(Vector2 worldPosition)
        {
            worldPosition = _gridScene.PositionFloatSnapToGrid(worldPosition, quaternion.identity);
            Vector2 localPoint = transform.InverseTransformPoint(worldPosition);
            Vector2[] oldPoints = polygonCollider2D.points;
            Vector2[] newPoints = new Vector2[oldPoints.Length + 1];
            Array.Copy(oldPoints, newPoints, oldPoints.Length);
            newPoints[oldPoints.Length] = localPoint;
            polygonCollider2D.points = newPoints;
            lineRenderer.positionCount = newPoints.Length;
            for (int i = 0; i < newPoints.Length; i++)
                lineRenderer.SetPosition(i, transform.TransformPoint(newPoints[i]));
            UpdateIntersectingSegments();
        }

        void RemovePoint(int index)
        {
            if (polygonCollider2D.points.Length <= 2) return;
            Vector2[] oldPoints = polygonCollider2D.points;
            Vector2[] newPoints = new Vector2[oldPoints.Length - 1];
            Array.Copy(oldPoints, 0, newPoints, 0, index);
            Array.Copy(oldPoints, index + 1, newPoints, index, oldPoints.Length - index - 1);
            polygonCollider2D.points = newPoints;
            lineRenderer.positionCount = newPoints.Length;
            for (int i = 0; i < newPoints.Length; i++)
                lineRenderer.SetPosition(i, transform.TransformPoint(newPoints[i]));
            UpdateIntersectingSegments();
        }

        int InsertPointBetween(int pointOne, int pointTwo, Vector2 worldPosition)
        {
            if (pointTwo != pointOne + 1)
            {
                return -1;
            }

            Vector2 localPoint = transform.InverseTransformPoint(worldPosition);
            Vector2[] oldPoints = polygonCollider2D.points;
            int insertIndex = pointTwo;
            Vector2[] newPoints = new Vector2[oldPoints.Length + 1];
            Array.Copy(oldPoints, 0, newPoints, 0, insertIndex);
            newPoints[insertIndex] = localPoint;
            Array.Copy(oldPoints, insertIndex, newPoints, insertIndex + 1, oldPoints.Length - insertIndex);
            polygonCollider2D.points = newPoints;
            lineRenderer.positionCount = newPoints.Length;
            for (int i = 0; i < newPoints.Length; i++)
                lineRenderer.SetPosition(i, transform.TransformPoint(newPoints[i]));
            UpdateIntersectingSegments();
            return insertIndex;
        }

        Vector3 GetMouseWorldPosition(float z = 0f)
        {
            Vector3 mousePos = UnityEngine.Input.mousePosition;
            mousePos.z = z - Camera.main.transform.position.z;
            return Camera.main.ScreenToWorldPoint(mousePos);
        }

        Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ab = b - a;
            float t = Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab);
            t = Mathf.Clamp01(t);
            return a + t * ab;
        }

        (int, int, float, Vector2) ClosestSegmentIndices(Vector2[] polyline, Vector2 point)
        {
            float minDistSqr = float.MaxValue;
            int bestA = 0, bestB = 1;
            Vector2 closestResult = Vector2.zero;
            for (int i = 0; i < polyline.Length - 1; i++)
            {
                Vector2 a = transform.TransformPoint(polyline[i]);
                Vector2 b = transform.TransformPoint(polyline[i + 1]);
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

        void Update()
        {
            Vector2 mouseWorldPos = GetMouseWorldPosition();
            var (pointOne, pointTwo, minDistSqr, closestPointOnEdge) =
                ClosestSegmentIndices(polygonCollider2D.points, mouseWorldPos);
            bool isNearEdge = minDistSqr <= distanceToEdge * distanceToEdge;
            selectionCube.SetActive(isNearEdge);
            if (isNearEdge)
            {
                Vector2 worldP1 = transform.TransformPoint(polygonCollider2D.points[pointOne]);
                Vector2 worldP2 = transform.TransformPoint(polygonCollider2D.points[pointTwo]);
                float distToP1 = Vector2.Distance(closestPointOnEdge, worldP1);
                float distToP2 = Vector2.Distance(closestPointOnEdge, worldP2);
                if (distToP1 <= distanceToCorner) selectionCube.transform.position = worldP1;
                else if (distToP2 <= distanceToCorner) selectionCube.transform.position = worldP2;
                else selectionCube.transform.position = closestPointOnEdge;
            }

            if (!draggedPointIndex.HasValue && UnityEngine.Input.GetMouseButtonDown(0))
            {
                bool isCtrlPressed =
                    UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl);
                if (isNearEdge)
                {
                    Vector2 worldP1 = transform.TransformPoint(polygonCollider2D.points[pointOne]);
                    Vector2 worldP2 = transform.TransformPoint(polygonCollider2D.points[pointTwo]);
                    bool nearP1 = Vector2.Distance(selectionCube.transform.position, worldP1) <= distanceToCorner;
                    bool nearP2 = Vector2.Distance(selectionCube.transform.position, worldP2) <= distanceToCorner;
                    if (isCtrlPressed)
                    {
                        if (nearP1 && polygonCollider2D.points.Length > 2) RemovePoint(pointOne);
                        else if (nearP2 && polygonCollider2D.points.Length > 2) RemovePoint(pointTwo);
                    }
                    else
                    {
                        if (nearP1) BeginDraggingPoint(pointOne);
                        else if (nearP2) BeginDraggingPoint(pointTwo);
                        else
                        {
                            int newIndex = InsertPointBetween(pointOne, pointTwo, closestPointOnEdge);
                            if (newIndex >= 0) BeginDraggingPoint(newIndex);
                        }
                    }
                }
            }

            UpdateDraggedPoint();
            UpdateIntersectingSegments();
        }

        void BeginDraggingPoint(int pointIndex) => draggedPointIndex = pointIndex;

        void UpdateDraggedPoint()
        {
            if (!draggedPointIndex.HasValue) return;
            if (UnityEngine.Input.GetMouseButton(0))
            {
                Vector3 mouseWorldPos = GetMouseWorldPosition();
                UpdatePositions(draggedPointIndex.Value, mouseWorldPos);
            }
            else draggedPointIndex = null;
        }

        void UpdateIntersectingSegments()
        {
            ClearIntersectingSegments();
            if (polygonCollider2D.points.Length < 4) return;
            Vector2[] worldPoints = new Vector2[polygonCollider2D.points.Length];
            for (int i = 0; i < polygonCollider2D.points.Length; i++)
                worldPoints[i] = transform.TransformPoint(polygonCollider2D.points[i]);
            HashSet<int> intersectingSegments = new HashSet<int>();
            for (int i = 0; i < worldPoints.Length - 1; i++)
            for (int j = i + 2; j < worldPoints.Length - 1; j++)
            {
                if (j == i + 1) continue;
                if (DoSegmentsIntersect(worldPoints[i], worldPoints[i + 1], worldPoints[j], worldPoints[j + 1]))
                {
                    intersectingSegments.Add(i);
                    intersectingSegments.Add(j);
                }
            }

            foreach (int idx in intersectingSegments) CreateIntersectingSegment(worldPoints[idx], worldPoints[idx + 1]);
        }

        void CreateIntersectingSegment(Vector2 start, Vector2 end)
        {
            GameObject obj = GetSegmentObject();
            LineRenderer lr = obj.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            obj.SetActive(true);
        }

        GameObject GetSegmentObject()
        {
            foreach (var obj in intersectingSegmentObjects)
                if (!obj.activeSelf)
                    return obj;
            GameObject newObj = new GameObject("IntersectingSegment");
            newObj.transform.SetParent(transform);
            var
                lr = newObj.AddComponent<LineRenderer>();
            lr.material = redMaterial;
            lr.startWidth = intersectingSegmentWidth;
            lr.endWidth = intersectingSegmentWidth;
            lr.numCornerVertices = 2;
            intersectingSegmentObjects.Add(newObj);
            return newObj;
        }

        void ClearIntersectingSegments()
        {
            foreach (var obj in intersectingSegmentObjects) obj.SetActive(false);
        }
        
        void OnDestroy()
        {
            foreach (var obj in intersectingSegmentObjects) Destroy(obj);
            Destroy(redMaterial);
            Destroy(circleMaterial);
        }

        static bool DoSegmentsIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
        {
            float o1 = Orientation(p1, q1, p2);
            float o2 = Orientation(p1, q1, q2);
            float o3 = Orientation(p2, q2, p1);
            float o4 = Orientation(p2, q2, q1);
            if (((o1 > Epsilon && o2 < -Epsilon)&(o1 < -Epsilon && o2 > Epsilon)) &&
                ((o3 > Epsilon && o4 < -Epsilon)&(o3 < -Epsilon && o4 > Epsilon))) return true;
            if (Mathf.Abs(o1) < Epsilon && OnSegment(p1, q1, p2)) return true;
            if (Mathf.Abs(o2) < Epsilon && OnSegment(p1, q1, q2)) return true;
            if (Mathf.Abs(o3) < Epsilon && OnSegment(p2, q2, p1)) return true;
            if (Mathf.Abs(o4) < Epsilon && OnSegment(p2, q2, q1)) return true;
            return false;
        }

        static float Orientation(Vector2 a, Vector2 b, Vector2 c) =>
            (b.y - a.y) * (c.x - b.x) - (b.x - a.x) * (c.y - b.y);

        static bool OnSegment(Vector2 a, Vector2 b, Vector2 c) => c.x <= Mathf.Max(a.x, b.x) + Epsilon &&
                                                                  c.x >= Mathf.Min(a.x, b.x) - Epsilon &&
                                                                  c.y <= Mathf.Max(a.y, b.y) + Epsilon &&
                                                                  c.y >= Mathf.Min(a.y, b.y) - Epsilon;
    }
}