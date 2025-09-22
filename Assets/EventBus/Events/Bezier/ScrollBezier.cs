using EventBus;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
{
    public struct ScrollBezier : IEvent
    {
        public float ScrollOffset { get; }

        public ScrollBezier(float scrollOffset)
        {
            ScrollOffset = scrollOffset;
        }
    }
}