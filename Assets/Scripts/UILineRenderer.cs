using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Radishmouse
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class UILineRenderer : MaskableGraphic
    {
        private RectTransform _rectTransform;
        [SerializeField] private Vector2[] points;

        [Range(0.1f, 100f)]
        public float thickness = 10f;

        // Кэшируем RectTransform один раз
        private RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = (RectTransform)transform;
                return _rectTransform;
            }
        }

        public void SetPoints(Vector2[] newPoints)
        {
            points = newPoints ?? new Vector2[0];

            // Обновляем размер ВНЕ OnPopulateMesh!
            if (points.Length > 0)
            {
                float maxX = Mathf.Max(Mathf.Abs(points.Max(p => p.x)), Mathf.Abs(points.Min(p => p.x)));
                float maxY = Mathf.Max(Mathf.Abs(points.Max(p => p.y)), Mathf.Abs(points.Min(p => p.y)));
                
                rectTransform.sizeDelta = new Vector2(maxX*2, maxY*2);
            }
            else
            {
                rectTransform.sizeDelta = Vector2.zero;
            }

            SetVerticesDirty();
        }

        public void SetThickness(float newThickness)
        {
            thickness = Mathf.Max(0.1f, newThickness);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (points == null || points.Length < 2)
                return;

            for (int i = 0; i < points.Length - 1; i++)
            {
                CreateLineSegment(points[i], points[i + 1], vh);

                int index = i * 5;

                vh.AddTriangle(index, index + 1, index + 3);
                vh.AddTriangle(index + 3, index + 2, index);

                if (i != 0)
                {
                    vh.AddTriangle(index, index - 1, index - 3);
                    vh.AddTriangle(index + 1, index - 1, index - 2);
                }
            }
        }

        private void CreateLineSegment(Vector2 point1, Vector2 point2, VertexHelper vh)
        {
            Vector2 direction = (point2 - point1).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * (thickness * 0.5f);

            Vector2 p1a = point1 + normal;
            Vector2 p1b = point1 - normal;
            Vector2 p2a = point2 + normal;
            Vector2 p2b = point2 - normal;

            Rect rect = rectTransform.rect;

            AddVertex(vh, p1b, rect);
            AddVertex(vh, p1a, rect);
            AddVertex(vh, p2b, rect);
            AddVertex(vh, p2a, rect);
            AddVertex(vh, point2, rect);
        }

        private void AddVertex(VertexHelper vh, Vector2 position, Rect rect)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = color;
            vertex.uv0 = GetUVCoords(position, rect);
            vh.AddVert(vertex);
        }

        private Vector2 GetUVCoords(Vector2 position, Rect rect)
        {
            if (rect.width == 0 || rect.height == 0)
                return Vector2.zero;

            float x = (position.x - rect.xMin) / rect.width;
            float y = (position.y - rect.yMin) / rect.height;
            return new Vector2(x, y);
        }
    }
}