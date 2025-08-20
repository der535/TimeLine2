using System;
using NaughtyAttributes;
using UnityEngine;

namespace TimeLine
{
    public class PolygonColliderVizualizer : MonoBehaviour
    {
        [SerializeField] private PolygonCollider2D _polygonCollider2D;
        [SerializeField] private LineRenderer _lineRenderer;

        [Button]
        private void Start()
        {
            _lineRenderer.positionCount = _polygonCollider2D.points.Length;

            for (var index = 0; index < _polygonCollider2D.points.Length; index++)
            {
                var point = _polygonCollider2D.points[index];
                _lineRenderer.SetPosition(index, new Vector3(point.x, point.y, 0.0f));
            }
        }
    }
}
