using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TimeLine.LevelEditor.SpriteLoader
{
    public class SetNativeSize : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private RectTransform rectTransform;

        public void Init()
        {
            if (image == null || image.sprite == null || rectTransform == null)
                return;

            Vector2 originalSize = new Vector2(image.sprite.rect.width, image.sprite.rect.height);

            float targetAspect = originalSize.x / originalSize.y;
            float containerAspect = rectTransform.sizeDelta.x / rectTransform.sizeDelta.y;

            Vector2 newSize;
            if (targetAspect > containerAspect)
            {
                // Ширина ограничивающая
                newSize = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.x / targetAspect);
            }
            else
            {
                // Высота ограничивающая
                newSize = new Vector2(rectTransform.sizeDelta.y * targetAspect, rectTransform.sizeDelta.y);
            }

            rectTransform.sizeDelta = newSize;
        }
    }
}