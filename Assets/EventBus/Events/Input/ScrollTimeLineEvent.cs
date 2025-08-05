using EventBus;

namespace TimeLine.EventBus.Events.Input
{
    public struct ScrollTimeLineEvent : IEvent
    {
        public float ScrollOffset { get; }

        public ScrollTimeLineEvent(float scrollOffset)
        {
            ScrollOffset = scrollOffset;
        }
    }
}