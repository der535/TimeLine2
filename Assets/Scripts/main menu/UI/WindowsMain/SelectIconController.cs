using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TimeLine
{
    public class SelectIconController : MonoBehaviour
    {
        [SerializeField] private List<EventTrigger> eventTriggers;
        [SerializeField] private RectTransform selector; // ваш выделитель (например, рамка или фон)

        private float _originalY;

        private void Start()
        {
            selector.gameObject.SetActive(false);
            
            if (selector != null)
                _originalY = selector.anchoredPosition.y;

            foreach (var trigger in eventTriggers)
            {
                if (trigger == null) continue;

                var rect = trigger.GetComponent<RectTransform>();
                if (rect == null) continue;

                // Enter
                var entryEnter = new EventTrigger.Entry();
                entryEnter.eventID = EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((eventData) =>
                {
                    if (selector != null)
                    {
                        selector.gameObject.SetActive(true);
                        selector.anchoredPosition = new Vector2(selector.anchoredPosition.x, rect.anchoredPosition.y - selector.sizeDelta.y / 2);
                    }
                });
                trigger.triggers.Add(entryEnter);

                // Exit
                var entryExit = new EventTrigger.Entry();
                entryExit.eventID = EventTriggerType.PointerExit;
                entryExit.callback.AddListener((eventData) =>
                {
                    if (selector != null)
                        selector.gameObject.SetActive(false);
                });
                trigger.triggers.Add(entryExit);
            }
        }
    }
}