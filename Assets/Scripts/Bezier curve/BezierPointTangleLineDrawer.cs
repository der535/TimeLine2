using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace TimeLine
{
    public class BezierPointTangleLineDrawer : MonoBehaviour
    {
        [SerializeField] private RectTransform point;
        [Space]
        [SerializeField] private RectTransform leftLine;
        [FormerlySerializedAs("rightTLine")] [SerializeField] private RectTransform rightLine;
        [Space]
        [SerializeField] private RectTransform leftTangent;
        [SerializeField] private RectTransform rightTangent;
        [Space]
        [SerializeField] private float lineWidth;

        [Button]
        internal void UpdatePosition()
        {
            print("Update tangle");
            // === Левая линия ===
            var leftDir = leftTangent.anchoredPosition; // это UnityEngine.Vector2
            leftLine.anchoredPosition = leftDir / 2f;

            float leftDistance = Vector2.Distance(Vector2.zero, leftDir);
            leftLine.sizeDelta = new Vector2(leftDistance, lineWidth);

            float leftAngle = Mathf.Atan2(leftDir.y, leftDir.x) * Mathf.Rad2Deg;
            leftLine.rotation = Quaternion.Euler(0, 0, leftAngle);


            // === Правая линия ===
            var rightDir = rightTangent.anchoredPosition;
            rightLine.anchoredPosition = rightDir / 2f;

            float rightDistance = Vector2.Distance(Vector2.zero, rightDir);
            rightLine.sizeDelta = new Vector2(rightDistance, lineWidth);

            float rightAngle = Mathf.Atan2(rightDir.y, rightDir.x) * Mathf.Rad2Deg;
            rightLine.rotation = Quaternion.Euler(0, 0, rightAngle);
        }
    }
}