using Radishmouse;
using UnityEngine;

namespace TimeLine.LevelEditor.TransformationSquare
{
    public class TransformationSquareView : MonoBehaviour
    {
        public UILineRenderer lineRenderer;
        [Space] public RectTransform circleLeftTop;
        public RectTransform circleRightTop;
        public RectTransform circleRightBottom;
        public RectTransform circleLeftBottom;
        [Space] public RectTransform canvas;
        public RectTransform parentRect;

        public bool GetActive() => lineRenderer.enabled;

        public void SetActive(bool active)
        {
            lineRenderer.enabled = active;
            circleLeftTop.gameObject.SetActive(active);
            circleRightTop.gameObject.SetActive(active);
            circleRightBottom.gameObject.SetActive(active);
            circleLeftBottom.gameObject.SetActive(active);
        }
    }
}