using EventBus;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
{
    public class PanEvent : IEvent
    {
        public float PanOffset { get; }

        public PanEvent(float pan)
        {
            PanOffset = pan;
        }
    }
}
