using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace TimeLine
{
    [RequireComponent(typeof(RectTransform))]
    public class BezierLineDrawer : MonoBehaviour
    {
        [SerializeField] private RectTransform currentTime;
        [SerializeField] private List<BezierPoint> _points;
        [SerializeField] private AnimationCurve _acnimationCurves;

        [SerializeField, Range(0f, 10f)] private float value;

        [Header("Визуализация")] [SerializeField]
        private int lineResolution = 30; // Точность прорисовки кривой

        [SerializeField] private Color lineColor = Color.cyan;
        [SerializeField] private float lineWidth = 0.1f;

        [SerializeField] private LineRenderer lineRenderer;

        public void Clear()
        {
            _points.Clear();
        }
        
        public void AddPoint(BezierPoint point)
        {
            _points.Add(point);
        }

        internal void SetActive(bool active)
        {
            lineRenderer.enabled = active;
        }

        private void InitializeLineRenderer()
        {
            // Настройка LineRenderer для UI
            lineRenderer.useWorldSpace = false; // Работаем в локальном пространстве
            lineRenderer.positionCount = 0;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.material = new Material(Shader.Find("UI/Default")) { color = lineColor };
            lineRenderer.sortingOrder = 1;
        }
        
        [Button]
        internal void UpdateBezierCurve()
        {
            if (_points == null || _points.Count < 2 || lineRenderer == null) return;

            int totalPoints = (_points.Count - 1) * lineResolution + 1;
            lineRenderer.positionCount = totalPoints;

            int index = 0;
            for (int i = 0; i < _points.Count - 1; i++)
            {
                BezierPoint start = _points[i];
                BezierPoint end = _points[i + 1];

                for (int j = 0; j < lineResolution; j++)
                {
                    float t = (float)j / lineResolution;
                    Vector2 anchoredPos = Bezier.GetPoint(
                        start.Point,
                        start.TangentRight,
                        end.TangentLeft,
                        end.Point,
                        t);

                    lineRenderer.SetPosition(index++, RectTransformToLineRendererPosition(anchoredPos));
                }
            }

            // Добавляем последнюю точку последнего сегмента
            Vector2 lastAnchoredPos = _points[_points.Count - 1].Point;
            lineRenderer.SetPosition(totalPoints - 1, RectTransformToLineRendererPosition(lastAnchoredPos));
        }

        private Vector3 RectTransformToLineRendererPosition(Vector2 anchoredPosition)
        {
            // Преобразуем anchoredPosition в локальную позицию относительно нашего RectTransform
            RectTransform rectTransform = (RectTransform)transform;
            Vector2 pivotOffset = new Vector2(rectTransform.pivot.x * rectTransform.rect.width,
                rectTransform.pivot.y * rectTransform.rect.height);

            // Локальная позиция = anchoredPosition - pivotOffset (если pivot не (0,0))
            Vector2 localPos = anchoredPosition - pivotOffset;

            return new Vector3(localPos.x, localPos.y, 0f);
        }
    }
}