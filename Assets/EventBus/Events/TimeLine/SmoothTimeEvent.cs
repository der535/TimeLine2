using EventBus;

namespace TimeLine.EventBus.Events.TimeLine
{
    public struct SmoothTimeEvent : IEvent
    {
        public float Time { get; }

        public SmoothTimeEvent(float time)
        {
            Time = time;
        }
    }
}