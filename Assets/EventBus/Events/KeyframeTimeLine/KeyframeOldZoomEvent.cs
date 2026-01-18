using EventBus;

namespace TimeLine.EventBus.Events.KeyframeTimeLine
{
    public class KeyframeOldZoomEvent : IEvent
    {
        public float OldZoom { get; }

        public KeyframeOldZoomEvent(float oldPan)
        {
            OldZoom = oldPan;
        }
    }
}
