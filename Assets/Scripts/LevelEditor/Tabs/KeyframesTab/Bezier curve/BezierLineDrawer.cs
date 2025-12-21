using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using TimeLine;
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
            ClearLines();

            foreach (var bezierData in _bezierDates)
            {
                if (bezierData.Points.Count < 2) continue;

                Vector2[] points = new Vector2[2];
                int totalPoints = 2;
                
                if (bezierData.Points[0].BezierDragPoint._keyframe.Interpolation ==
                    Keyframe.Keyframe.InterpolationType.Bezier)
                {
                    totalPoints = (bezierData.Points.Count - 1) * lineResolution + 1;

                    points = new Vector2[totalPoints];

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

                            points[index++] = anchoredPos;
                        }
                    }
                }
                else if(bezierData.Points[0].BezierDragPoint._keyframe.Interpolation ==
                        Keyframe.Keyframe.InterpolationType.Linear)
                {
                    for (int i = 0; i < bezierData.Points.Count - 1; i++)
                    {
                        BezierPoint start = bezierData.Points[i];
                        BezierPoint end = bezierData.Points[i + 1];

                        // Пропускаем уничтоженные точки
                        if (start == null || end == null) continue;

                        points[0] = start.Point;
                        points[1] = end.Point;
                    }
                }
                else if(bezierData.Points[0].BezierDragPoint._keyframe.Interpolation ==
                        Keyframe.Keyframe.InterpolationType.Hold)
                {
                    for (int i = 0; i < bezierData.Points.Count - 1; i++)
                    {
                        points = new Vector2[3];
                        totalPoints = 3;
                        BezierPoint start = bezierData.Points[i];
                        BezierPoint end = bezierData.Points[i + 1];

                        // Пропускаем уничтоженные точки
                        if (start == null || end == null) continue;

                        points[0] = start.Point;
                        points[1] = new Vector2(end.Point.x, start.Point.y);
                        points[2] = end.Point;
                    }
                }

                BezierCurve curve = Instantiate(curvePrefab, root);
                curve.Setup(lineWidth, bezierData.BezierColor);

                if (bezierData.Points[^1] != null)
                {
                    Vector2 lastAnchoredPos = bezierData.Points[^1].Point;
                    points[totalPoints - 1] = lastAnchoredPos;
                }

                curve.UpdatePoints(points);
                bezierLines.Add(curve);
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