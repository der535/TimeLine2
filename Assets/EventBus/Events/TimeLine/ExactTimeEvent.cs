using EventBus;

namespace TimeLine.EventBus.Events.TimeLine
{
    public struct ExactTimeEvent : IEvent
    {
        public float Time { get; }

        public ExactTimeEvent(float time)
        {
            Time = time;
        }
    }
}