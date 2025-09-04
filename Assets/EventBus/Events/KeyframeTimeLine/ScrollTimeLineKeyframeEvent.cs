using EventBus;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
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