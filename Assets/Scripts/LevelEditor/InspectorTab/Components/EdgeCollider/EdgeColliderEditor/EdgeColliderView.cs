using System.Collections.Generic;
using UnityEngine;

namespace TimeLine.EdgeColliderEditor
{
    public class EdgeColliderView : MonoBehaviour
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
        private const int CircleResolution = 20;

        public void Initialize(Transform colliderTransform, LineRenderer outlineRenderer)
        {
            ColliderTransform = colliderTransform;
            OutlineRenderer = outlineRenderer;
            OutlineRenderer.sortingOrder = 1000001;
            
            RedMaterial = new Material(Shader.Find("Sprites/Default")) { color = Color.red };
            CircleMaterial = new Material(Shader.Find("Sprites/Default")) { color = new Color(0, 1, 0, 0.5f) };
        }

        public void UpdateOutline(List<Vector2> points)
        {
            OutlineRenderer.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                OutlineRenderer.SetPosition(i, ColliderTransform.TransformPoint(points[i]));
            }
        }

        public void UpdateIntersectingSegments(List<Vector2> worldPoints)
        {
            ClearIntersectingSegments();
            if (worldPoints.Count < 4) return;

            var intersectingSegments = new HashSet<int>();
            for (int i = 0; i < worldPoints.Count - 1; i++)
            {
                for (int j = i + 2; j < worldPoints.Count - 1; j++)
                {
                    if (j == i + 1) continue;
                    if (DoSegmentsIntersect(worldPoints[i], worldPoints[i + 1], worldPoints[j], worldPoints[j + 1]))
                    {
                        intersectingSegments.Add(i);
                        intersectingSegments.Add(j);
                    }
                }
            }

            foreach (int idx in intersectingSegments)
                CreateIntersectingSegment(worldPoints[idx], worldPoints[idx + 1]);
        }

        public void UpdateEdgeRadiusVisualization(List<Vector2> worldPoints, float radius)
        {
            if (radius <= 0)
            {
                foreach (var lr in RadiusVisualizers)
                    lr.gameObject.SetActive(false);
                return;
            }

            while (RadiusVisualizers.Count < worldPoints.Count)
            {
                var obj = new GameObject("RadiusCircle");
                obj.transform.SetParent(ColliderTransform);
                var lr = obj.AddComponent<LineRenderer>();
                lr.material = CircleMaterial;
                lr.startWidth = lr.endWidth = IntersectingSegmentWidthDynamic;
                lr.positionCount = CircleResolution + 1;
                RadiusVisualizers.Add(lr);
            }

            for (int i = 0; i < worldPoints.Count; i++)
            {
                var lr = RadiusVisualizers[i];
                lr.gameObject.SetActive(true);
                Vector3[] positions = new Vector3[CircleResolution + 1];
                for (int j = 0; j <= CircleResolution; j++)
                {
                    float angle = 2 * Mathf.PI * j / CircleResolution;
                    positions[j] = worldPoints[i] + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
                }
                lr.SetPositions(positions);
            }

            for (int i = worldPoints.Count; i < RadiusVisualizers.Count; i++)
                RadiusVisualizers[i].gameObject.SetActive(false);
        }

        public void UpdateEdgeRadiusConnections(List<Vector2> worldPoints, float radius)
        {
            if (radius <= 0 || worldPoints.Count < 2)
            {
                foreach (var lr in EdgeConnectionVisualizers)
                    lr.gameObject.SetActive(false);
                return;
            }

            int segmentCount = worldPoints.Count - 1;
            while (EdgeConnectionVisualizers.Count < segmentCount * 2)
            {
                var obj = new GameObject("EdgeConnection");
                obj.transform.SetParent(ColliderTransform);
                var lr = obj.AddComponent<LineRenderer>();
                lr.material = CircleMaterial;
                lr.startWidth = lr.endWidth = IntersectingSegmentWidthDynamic;
                lr.positionCount = 2;
                EdgeConnectionVisualizers.Add(lr);
            }

            int connectionIndex = 0;
            for (int i = 0; i < segmentCount; i++)
            {
                Vector2 p1 = worldPoints[i];
                Vector2 p2 = worldPoints[i + 1];
                Vector2 segment = p2 - p1;
                Vector2 normal = new Vector2(-segment.y, segment.x).normalized;

                Vector3 leftStart = p1 + normal * radius;
                Vector3 leftEnd = p2 + normal * radius;
                Vector3 rightStart = p1 - normal * radius;
                Vector3 rightEnd = p2 - normal * radius;

                var leftLr = EdgeConnectionVisualizers[connectionIndex++];
                leftLr.gameObject.SetActive(true);
                leftLr.SetPosition(0, leftStart);
                leftLr.SetPosition(1, leftEnd);

                var rightLr = EdgeConnectionVisualizers[connectionIndex++];
                rightLr.gameObject.SetActive(true);
                rightLr.SetPosition(0, rightStart);
                rightLr.SetPosition(1, rightEnd);
            }

            for (int i = connectionIndex; i < EdgeConnectionVisualizers.Count; i++)
                EdgeConnectionVisualizers[i].gameObject.SetActive(false);
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