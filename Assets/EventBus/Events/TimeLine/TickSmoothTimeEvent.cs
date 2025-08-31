using EventBus;

namespace TimeLine.EventBus.Events.TimeLine
{
    public struct TickSmoothTimeEvent : IEvent
    {
        public double Time { get; }

        public TickSmoothTimeEvent(double time)
        {
            Time = time;
        }
    }
}