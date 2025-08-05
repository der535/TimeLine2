using EventBus;
using UnityEngine;

namespace TimeLine.EventBus.Events.Misc
{
    public class ContentRectTransformChangedEvent : IEvent
    {
        public RectTransform ContentRectTransform { get; }

        public ContentRectTransformChangedEvent(RectTransform contentRectTransform)
        {
            ContentRectTransform = contentRectTransform;
        }
    }
}