using TimeLine.LevelEditor.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TimeLine.LevelEditor.UIAnimation
{
    public class PressButtonScale : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private EventTrigger eventTrigger;
        [SerializeField, Range(1, 100)] private float pressScalePercent;

        private Vector2 _initialScale;
        private Vector2 _pressScale;

        private void Start()
        {
            _initialScale = rectTransform.sizeDelta;
            _pressScale = _initialScale * (pressScalePercent / 100);
            UIUtils.AddPointerListener(eventTrigger, EventTriggerType.PointerDown,
                () => { rectTransform.sizeDelta = _pressScale; });
            UIUtils.AddPointerListener(eventTrigger, EventTriggerType.PointerUp,
                () => { rectTransform.sizeDelta = _initialScale; });
        }
    }
}