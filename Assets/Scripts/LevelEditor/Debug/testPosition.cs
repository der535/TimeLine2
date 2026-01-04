using System;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class testPosition : MonoBehaviour
    {
        [Header("Ссылки")]
        public Camera sceneCamera;        // Камера, которая смотрит на объект (Render Texture)
        public RawImage displayImage;     // UI элемент, где лежит Render Texture
        public Transform targetObject;    // 3D объект в мире
        public RectTransform uiMarker;    // Ваш UI маркер (иконка)

        [Header("Смещение")]
        public Vector3 worldOffset;       // Смещение в 3D (например, чтобы маркер был над головой)

        void LateUpdate()
        {
            if (!sceneCamera || !displayImage || !targetObject || !uiMarker) return;

            // 1. Получаем вьюпорт-координаты объекта (от 0 до 1)
            // (0.5, 0.5) — это центр кадра камеры
            Vector3 viewportPoint = sceneCamera.WorldToViewportPoint(targetObject.position + worldOffset);
            

            // 2. Считаем локальную позицию внутри RawImage
            RectTransform rawRect = displayImage.rectTransform;
        
            // Переводим Viewport Point в координаты Rect, учитывая Pivot (точку опоры) RawImage
            float localX = (viewportPoint.x - rawRect.pivot.x) * rawRect.rect.width;
            float localY = (viewportPoint.y - rawRect.pivot.y) * rawRect.rect.height;
            Vector2 localPos = new Vector2(localX, localY);

            // 3. САМОЕ ВАЖНОЕ: Переводим локальную точку RawImage в мировое пространство
            // Это учитывает, где именно на экране находится сам RawImage, его масштаб и наклон
            Vector3 worldPoint = rawRect.TransformPoint(localPos);

            // 4. Устанавливаем позицию маркеру
            // Теперь маркеру все равно, какая у него иерархия, он привязан к позиции в мире экрана
            uiMarker.position = worldPoint;
        }
    }
}