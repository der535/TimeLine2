using System;
using UnityEngine;

namespace TimeLine
{
    public class PixelPerfectSize : MonoBehaviour
    {
        [SerializeField] private float size = 1f;
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform rectTransform;
        private void Start()
        {
            float pixelWidth = size / canvas.scaleFactor;
            rectTransform.sizeDelta = new Vector2(pixelWidth, rectTransform.sizeDelta.y);
        }
    }
}
