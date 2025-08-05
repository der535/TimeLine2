using EventBus;

namespace TimeLine.EventBus.Events.TimeLine
{
    public struct BeatEvent : IEvent
    {
        public float Beat { get; }

        public BeatEvent(float time)
        {
            Beat = time;
        }
    }
}