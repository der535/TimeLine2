using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using TimeLine;
using TimeLine.LevelEditor.Core;
using UnityEngine;

namespace TimeLine
{
    [RequireComponent(typeof(RectTransform))]
    public class BezierLineDrawer : MonoBehaviour
    {
        [SerializeField, Range(0f, 10f)] private float value;

        [Header("Визуализация")] [SerializeField]
        private int lineResolution = 30;

        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private RectTransform pointsRoot;
        [Space] [SerializeField] private BezierCurve curvePrefab;
        [SerializeField] private RectTransform root;
        [Space] [SerializeField] private List<BezierCurve> bezierLines;

        private List<BezierData> _bezierDates = new();


        internal void ClearLines()
        {
            foreach (var line in bezierLines.ToArray())
            {
                bezierLines.Remove(line);
                Destroy(line.gameObject);
            }

            bezierLines.Clear();
        }

        internal void ClearBeziers()
        {
            _bezierDates.Clear();
        }

        internal void SortPoints()
        {
            foreach (var data in _bezierDates)
            {
                data.Points.Sort((x, y) =>
                    x.BezierDragPoint._keyframe.Ticks.CompareTo(y.BezierDragPoint._keyframe.Ticks));
            }
        }

        public void AddPoints(List<BezierPoint> points, Color bezierColor)
        {
            // print("Добавление точек");
            // Защита от null и очистка уничтоженных точек
            var validPoints = points.Where(p => p != null && p.transform != null).ToList();
            if (validPoints.Count < 2) return;

            _bezierDates.Add(new BezierData
            {
                BezierColor = bezierColor,
                Points = validPoints
            });

            // print(_bezierDates.Count);
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
            ClearLines();

            foreach (var bezierData in _bezierDates)
            {
                if (bezierData.Points == null || bezierData.Points.Count < 2) continue;

                // Используем List, чтобы не гадать с размером массива
                List<Vector2> allPoints = new List<Vector2>();

                for (int i = 0; i < bezierData.Points.Count - 1; i++)
                {
                    BezierPoint start = bezierData.Points[i];
                    BezierPoint end = bezierData.Points[i + 1];

                    if (start == null || end == null) continue;

                    // Определяем интерполяцию для текущего отрезка
                    var interpolation = start.BezierDragPoint._keyframe.Interpolation;

                    if (interpolation == Keyframe.Keyframe.InterpolationType.Bezier)
                    {
                        // Для Безье добавляем много точек (сглаживание)
                        for (int j = 0; j < lineResolution; j++)
                        {
                            float t = (float)j / lineResolution;
                            allPoints.Add(TimeLineConverter.GetPointBezier(
                                start.Point,
                                start.TangentRight,
                                end.TangentLeft,
                                end.Point,
                                t));
                        }
                    }
                    else if (interpolation == Keyframe.Keyframe.InterpolationType.Linear)
                    {
                        // Для Линейной добавляем только начало отрезка
                        allPoints.Add(start.Point);
                    }
                    else if (interpolation == Keyframe.Keyframe.InterpolationType.Hold)
                    {
                        // Для Холд добавляем начало и "угол" ступеньки
                        allPoints.Add(start.Point);
                        allPoints.Add(new Vector2(end.Point.x, start.Point.y));
                    }
                }

                // КРИТИЧЕСКИ ВАЖНО: Добавляем самую последнюю точку всей цепи
                var lastPoint = bezierData.Points[^1];
                if (lastPoint != null)
                {
                    allPoints.Add(lastPoint.Point);
                }

                // Создаем объект и передаем данные
                if (allPoints.Count >= 2)
                {
                    BezierCurve curve = Instantiate(curvePrefab, root);
                    curve.Setup(lineWidth, bezierData.BezierColor);
                    curve.UpdatePoints(allPoints.ToArray());
                    bezierLines.Add(curve);
                }
            }
        }
    }
}

class BezierData
{
    public List<BezierPoint> Points = new();
    public BezierCurve BezierCurve;
    public Color BezierColor;
}