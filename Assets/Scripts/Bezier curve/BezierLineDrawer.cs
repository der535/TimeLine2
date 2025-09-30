using System.Collections.Generic;
using System.Linq; // Добавьте это для использования LINQ
using NaughtyAttributes;
using TimeLine;
using UnityEngine;

namespace TimeLine
{
    [RequireComponent(typeof(RectTransform))]
    public class BezierLineDrawer : MonoBehaviour
    {
        private List<BezierData> _bezierDates = new();

        [SerializeField, Range(0f, 10f)] private float value;

        [Header("Визуализация")] [SerializeField]
        private int lineResolution = 30;

        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private RectTransform pointsRoot;
        [Space] [SerializeField] private LineRenderer linePrefab;
        [SerializeField] private RectTransform root;

        [Space] [SerializeField] private List<LineRenderer> bezierLines;

        internal void ClearLines()
        {
            // print("Очистка");

            foreach (var line in bezierLines.ToArray())
            {
                bezierLines.Remove(line);
                Destroy(line.gameObject);
            }

            bezierLines.Clear();
            // print(bezierLines.Count);
        }

        internal void ClearBeziers()
        {
            _bezierDates.Clear();
        }

        private LineRenderer CreateLine(Color color)
        {
            print("Линия");
            
            LineRenderer line = Instantiate(linePrefab, root);
            GradientColorKey[] colorKeys = new[] { new GradientColorKey(color, 0), new GradientColorKey(color, 1) };
            Gradient gradient = new Gradient { colorKeys = colorKeys };
            line.colorGradient = gradient;
            bezierLines.Add(line);
            return line;
        }

        public void AddPoints(List<BezierPoint> points, Color bezierColor)
        {
            print("Добавление точек");
            // Защита от null и очистка уничтоженных точек
            var validPoints = points.Where(p => p != null && p.transform != null).ToList();
            if (validPoints.Count < 2) return;

            _bezierDates.Add(new BezierData
            {
                BezierColor = bezierColor,
                Points = validPoints
            });
            
            print(_bezierDates.Count);
        }

        internal void SetActive(bool active)
        {
            foreach (var line in bezierLines.Where(line => line != null))
            {
                line.enabled = active;
            }
        }

        [Button]
        internal void UpdateBezierCurve()
        {
            // print("UpdateBezierCurve");
            
            ClearLines();

            // print(_bezierDates.Count);

            
            // print(_bezierDates.Count);

            foreach (var bezierData in _bezierDates)
            {
                if (bezierData.Points.Count < 2) continue;

                int totalPoints = (bezierData.Points.Count - 1) * lineResolution + 1;
                LineRenderer line = CreateLine(bezierData.BezierColor);
                line.positionCount = totalPoints;

                int index = 0;
                for (int i = 0; i < bezierData.Points.Count - 1; i++)
                {
                    BezierPoint start = bezierData.Points[i];
                    BezierPoint end = bezierData.Points[i + 1];

                    // Пропускаем уничтоженные точки
                    if (start == null || end == null) continue;

                    for (int j = 0; j < lineResolution; j++)
                    {
                        float t = (float)j / lineResolution;
                        Vector2 anchoredPos = Bezier.GetPoint(
                            start.Point,
                            start.TangentRight,
                            end.TangentLeft,
                            end.Point,
                            t);

                        line.SetPosition(index++, RectTransformToLineRendererPosition(anchoredPos));
                    }
                }

                // Защита для последней точки
                if (bezierData.Points[^1] != null)
                {
                    Vector2 lastAnchoredPos = bezierData.Points[^1].Point;
                    line.SetPosition(totalPoints - 1, RectTransformToLineRendererPosition(lastAnchoredPos));
                }
            }
            

        }

        private Vector3 RectTransformToLineRendererPosition(Vector2 anchoredPosition)
        {
            if (pointsRoot == null) return anchoredPosition;

            RectTransform rectTransform = (RectTransform)transform;
            Vector2 pivotOffset = new Vector2(
                rectTransform.pivot.x * rectTransform.rect.width,
                rectTransform.pivot.y * rectTransform.rect.height
            );

            Vector2 localPos = anchoredPosition - pivotOffset;
            return new Vector3(localPos.x, localPos.y + pointsRoot.offsetMin.y, 0f);
        }
    }
}

class BezierData
{
    public List<BezierPoint> Points = new();
    public Color BezierColor;
}