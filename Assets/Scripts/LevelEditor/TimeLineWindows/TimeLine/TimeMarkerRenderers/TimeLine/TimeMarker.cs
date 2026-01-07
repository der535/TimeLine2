using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class TimeMarker : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image image;

        private bool _isVertical;

        private Canvas canvas;

        public RectTransform RectTransform => rectTransform;
        public void Setup(Canvas canvas, string time, Color color)
        {
            image.color = color;
            this.canvas = canvas;
            text.text = time;
            float pixelWidth = 1f / canvas.scaleFactor;
            rectTransform.sizeDelta = new Vector2(pixelWidth, rectTransform.sizeDelta.y);
        }
#if UNITY_EDITOR
        void Update()
        {
            UpdateTick();
        }
#endif
        void UpdateTick()
        {
            if (canvas == null || rectTransform == null) return;

            // Устанавливаем ширину ровно в 1 пиксель с учетом масштаба
            float pixelWidth = 1f / canvas.scaleFactor;
            rectTransform.sizeDelta = new Vector2(pixelWidth, rectTransform.sizeDelta.y);
        }
    }
}