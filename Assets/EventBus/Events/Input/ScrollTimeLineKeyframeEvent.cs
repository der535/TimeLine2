using EventBus;

namespace TimeLine.EventBus.Events.Input
{
    public struct ScrollTimeLineKeyframeEvent : IEvent
    {
        public float ScrollOffset { get; }

        public ScrollTimeLineKeyframeEvent(float scrollOffset)
        {
            ScrollOffset = scrollOffset;
        }
    }
}