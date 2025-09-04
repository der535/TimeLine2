using EventBus;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
{
    public class OldPanEvent : IEvent
    {
        public float OldPanOffset { get; }

        public OldPanEvent(float oldPan)
        {
            OldPanOffset = oldPan;
        }
    }
}
