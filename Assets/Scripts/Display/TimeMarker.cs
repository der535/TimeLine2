using System;
using System.Globalization;
using TimeLine.Installers;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class TimeMarker : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private RectTransform rectTransform;

        private Canvas canvas;
        

        public void Setup(Canvas canvas, float t)
        {
            this.canvas = canvas;
            text.text = t.ToString(CultureInfo.InvariantCulture);
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
