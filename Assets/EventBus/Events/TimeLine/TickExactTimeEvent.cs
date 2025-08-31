using EventBus;

namespace TimeLine.EventBus.Events.TimeLine
{
    public struct TickExactTimeEvent : IEvent
    {
        public double Time { get; }

        public TickExactTimeEvent(double time)
        {
            Time = time;
        }
    }
}