using EventBus;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
{
    public class KeyframeZoomEvent : IEvent
    {
        public float PanOffset { get; }

        public KeyframeZoomEvent(float pan)
        {
            PanOffset = pan;
        }
    }
}
