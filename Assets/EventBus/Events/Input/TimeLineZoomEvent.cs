using EventBus;

namespace TimeLine.EventBus.Events.Input
{
    public class TimeLineZoomEvent : IEvent
    {
        public float PanOffset { get; }

        public TimeLineZoomEvent(float pan)
        {
            PanOffset = pan;
        }
    }
}
