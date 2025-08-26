using UnityEngine;

namespace TimeLine.CustomInspector.UI
{
    public class FieldSpace : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        
        public void Setup(float value)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, value);
        }
    }
}