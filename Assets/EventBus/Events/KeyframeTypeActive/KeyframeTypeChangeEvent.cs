using EventBus;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
{
    public class KeyframeTypeChangeEvent : IEvent
    {
        public M_KeyframeType ActiveType { get; }

        public KeyframeTypeChangeEvent(M_KeyframeType activeType)
        {
            ActiveType = activeType;
        }
    }
}
