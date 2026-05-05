using System.Collections.Generic;
using UnityEngine;

namespace TimeLine
{
    public class TabBar : MonoBehaviour
    {
        private static readonly List<TabBar> _all = new();
        public static IReadOnlyList<TabBar> All => _all;

        [SerializeField] private RectTransform _container; // c HorizontalLayoutGroup
        [SerializeField] private float _hoverInflate = 80f;

        public RectTransform Container => _container;

        private RectTransform _selfRect;

        private void Awake() => _selfRect = (RectTransform)transform;
        private void OnEnable() => _all.Add(this);
        private void OnDisable() => _all.Remove(this);

        public bool ContainsScreenPointStrict(Vector2 screenPos, Camera cam)
            => RectTransformUtility.RectangleContainsScreenPoint(_selfRect, screenPos, cam);

        public bool ContainsScreenPointInflated(Vector2 screenPos, Camera cam)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_selfRect, screenPos, cam, out var local))
                return false;
            var r = _selfRect.rect;
            r.xMin -= _hoverInflate; r.xMax += _hoverInflate;
            r.yMin -= _hoverInflate; r.yMax += _hoverInflate;
            return r.Contains(local);
        }

        public float DistanceToScreenPoint(Vector2 screenPos, Camera cam)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_selfRect, screenPos, cam, out var local))
                return float.MaxValue;
            var r = _selfRect.rect;
            var closest = new Vector2(
                Mathf.Clamp(local.x, r.xMin, r.xMax),
                Mathf.Clamp(local.y, r.yMin, r.yMax));
            return Vector2.Distance(local, closest);
        }

        public int CalculateInsertIndex(Vector2 screenPos, Camera cam, Transform exclude)
        {
            int idx = 0;
            var corners = new Vector3[4];
            for (int i = 0; i < _container.childCount; i++)
            {
                var child = _container.GetChild(i);
                if (child == exclude || !child.gameObject.activeSelf) continue;
                ((RectTransform)child).GetWorldCorners(corners);
                var screenCenter = RectTransformUtility.WorldToScreenPoint(cam, (corners[0] + corners[2]) * 0.5f);
                if (screenPos.x < screenCenter.x) return idx;
                idx++;
            }
            return idx;
        }
    }
}