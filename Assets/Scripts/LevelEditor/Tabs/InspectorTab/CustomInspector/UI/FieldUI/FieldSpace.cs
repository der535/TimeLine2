using TimeLine.CustomInspector.UI.FieldUI;
using UnityEngine;

namespace TimeLine.CustomInspector.UI
{
    public class FieldSpace : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform fieldRect;
        [Space]
        [SerializeField] private RectTransform rectTransform;
        
        public void Setup(float value)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, value);
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }
    }
}