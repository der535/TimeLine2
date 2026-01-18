using System.Collections.Generic;
using UnityEngine;

namespace TimeLine.EdgeColliderEditor
{
    public class PolygonColliderView : MonoBehaviour
    {
        // Core references
        public Transform ColliderTransform { get; private set; }
        public LineRenderer OutlineRenderer { get; private set; }

        // Visual materials
        public Material RedMaterial { get; private set; }
        public Material CircleMaterial { get; private set; }

        // Dynamic parameters (updated by Host)
        public float IntersectingSegmentWidthDynamic;
        public float DistanceToCornerDynamic;
        public float DistanceToEdgeDynamic;

        // Visual elements
        public List<GameObject> IntersectingSegmentObjects = new();
        public List<LineRenderer> RadiusVisualizers = new();
        public List<LineRenderer> EdgeConnectionVisualizers = new();

        // Constants
        private const float Epsilon = 0.0001f;

        public void Initialize(Transform colliderTransform, LineRenderer outlineRenderer)
        {
            ColliderTransform = colliderTransform;
            OutlineRenderer = outlineRenderer;
            OutlineRenderer.loop = true;

            RedMaterial = new Material(Shader.Find("Sprites/Default")) { color = Color.red };
            CircleMaterial = new Material(Shader.Find("Sprites/Default")) { color = new Color(0, 1, 0, 0.5f) };
        }

        public void UpdateOutline(List<Vector2> points)
        {
            // Для полигона всегда делаем замкнутый контур
            // НОВЫЙ КОД (ТОЛЬКО РЕАЛЬНЫЕ ТОЧКИ):
            OutlineRenderer.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                OutlineRenderer.SetPosition(i, ColliderTransform.TransformPoint(points[i]));
            }
        }

        // В UpdateIntersectingSegments тоже используем замыкание:
        public void UpdateIntersectingSegments(List<Vector2> worldPoints)
        {
            ClearIntersectingSegments();
            if (worldPoints.Count < 4) return;

            var intersectingSegments = new HashSet<(int, int)>();
            int pointCount = worldPoints.Count;

            for (int i = 0; i < pointCount; i++)
            {
                int nextI = (i + 1) % pointCount; // <-- Замыкание контура
                for (int j = 0; j < pointCount; j++)
                {
                    if (i == j) continue;
                    int nextJ = (j + 1) % pointCount;

                    // Пропускаем смежные сегменты
                    if (nextI == j || nextJ == i || (i == nextJ && j == nextI)) continue;

                    if (DoSegmentsIntersect(
                            worldPoints[i], worldPoints[nextI],
                            worldPoints[j], worldPoints[nextJ]))
                    {
                        intersectingSegments.Add((i, nextI));
                        intersectingSegments.Add((j, nextJ));
                    }
                }
            }

            foreach (var (start, end) in intersectingSegments)
            {
                CreateIntersectingSegment(worldPoints[start], worldPoints[end]);
            }
        }


        // --- Helper Methods ---
        private void CreateIntersectingSegment(Vector2 start, Vector2 end)
        {
            GameObject obj = GetSegmentObject();
            LineRenderer lr = obj.GetComponent<LineRenderer>();
            lr.sortingOrder = 1;
            lr.startWidth = IntersectingSegmentWidthDynamic;
            lr.endWidth = IntersectingSegmentWidthDynamic;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            obj.SetActive(true);
        }

        private GameObject GetSegmentObject()
        {
            foreach (var obj in IntersectingSegmentObjects)
            {
                if (!obj.activeSelf)
                    return obj;
            }

            GameObject newObj = new GameObject("IntersectingSegment");
            newObj.transform.SetParent(ColliderTransform);
            var lr = newObj.AddComponent<LineRenderer>();
            lr.material = RedMaterial;
            IntersectingSegmentObjects.Add(newObj);
            return newObj;
        }

        private void ClearIntersectingSegments()
        {
            foreach (var obj in IntersectingSegmentObjects)
                obj.SetActive(false);
        }

        // Segment intersection logic
        private static bool DoSegmentsIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
        {
            float o1 = Orientation(p1, q1, p2);
            float o2 = Orientation(p1, q1, q2);
            float o3 = Orientation(p2, q2, p1);
            float o4 = Orientation(p2, q2, q1);

            if (((o1 > Epsilon && o2 < -Epsilon) || (o1 < -Epsilon && o2 > Epsilon)) &&
                ((o3 > Epsilon && o4 < -Epsilon) || (o3 < -Epsilon && o4 > Epsilon)))
                return true;

            if (Mathf.Abs(o1) < Epsilon && OnSegment(p1, q1, p2)) return true;
            if (Mathf.Abs(o2) < Epsilon && OnSegment(p1, q1, q2)) return true;
            if (Mathf.Abs(o3) < Epsilon && OnSegment(p2, q2, p1)) return true;
            if (Mathf.Abs(o4) < Epsilon && OnSegment(p2, q2, q1)) return true;

            return false;
        }

        private static float Orientation(Vector2 a, Vector2 b, Vector2 c) =>
            (b.y - a.y) * (c.x - b.x) - (b.x - a.x) * (c.y - b.y);

        private static bool OnSegment(Vector2 a, Vector2 b, Vector2 c) =>
            c.x <= Mathf.Max(a.x, b.x) + Epsilon &&
            c.x >= Mathf.Min(a.x, b.x) - Epsilon &&
            c.y <= Mathf.Max(a.y, b.y) + Epsilon &&
            c.y >= Mathf.Min(a.y, b.y) - Epsilon;

        private void OnDestroy()
        {
            foreach (var obj in IntersectingSegmentObjects)
                Destroy(obj);

            foreach (var lr in RadiusVisualizers)
                Destroy(lr.gameObject);

            foreach (var lr in EdgeConnectionVisualizers)
                Destroy(lr.gameObject);

            Destroy(RedMaterial);
            Destroy(CircleMaterial);
        }
    }
}