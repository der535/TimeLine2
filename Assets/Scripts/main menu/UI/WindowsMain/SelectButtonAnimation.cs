using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TimeLine
{
    public class SelectButtonAnimation : MonoBehaviour
    {
        [SerializeField] private RectTransform buttonRectTransform;
        [SerializeField] private EventTrigger eventTrigger;
        [SerializeField] private float addHeightOnHover;
        [SerializeField] private float duractionAnimation;
        
        private float _basePositionX;
        private float _baseWight;

        private float currentValue;

        private void Start()
        {
            _basePositionX = buttonRectTransform.anchoredPosition.x;
            _baseWight = buttonRectTransform.sizeDelta.x;
            
            EventTrigger trigger = GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => { Enter((PointerEventData)data); });
            trigger.triggers.Add(entry);
            
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener((data) => { Exit((PointerEventData)data); });
            trigger.triggers.Add(entry);
        }

        private void Enter(PointerEventData data)
        {
            DOVirtual.Float(currentValue, addHeightOnHover, duractionAnimation, (value) =>
            {
                currentValue = value;
                buttonRectTransform.anchoredPosition = new Vector2(_basePositionX + value/2, buttonRectTransform.anchoredPosition.y);
                buttonRectTransform.sizeDelta = new Vector2(_baseWight + value, buttonRectTransform.sizeDelta.y);
            });
        }
        
        private void Exit(PointerEventData data)
        {
            DOVirtual.Float(currentValue, 0, duractionAnimation, (value) =>
            {
                currentValue = value;
                buttonRectTransform.anchoredPosition = new Vector2(_basePositionX + value/2, buttonRectTransform.anchoredPosition.y);
                buttonRectTransform.sizeDelta = new Vector2(_baseWight + value, buttonRectTransform.sizeDelta.y);
            });
        }
    }
}
