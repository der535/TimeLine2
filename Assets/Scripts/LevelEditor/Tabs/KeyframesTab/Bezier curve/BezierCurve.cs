using Radishmouse;
using UnityEngine;

namespace TimeLine
{
    public class BezierCurve : MonoBehaviour
    {
        [SerializeField] private UILineRenderer lineRenderer;

        internal void Setup(float thickness, Color color)
        {
            lineRenderer.thickness = thickness;
            lineRenderer.color = color;
        }

        internal void UpdatePoints(Vector2[] points)
        {
            lineRenderer.SetPoints(points);
        }
    }
}
