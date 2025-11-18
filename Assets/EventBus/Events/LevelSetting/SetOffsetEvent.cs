using EventBus;

namespace TimeLine.EventBus.Events.Input
{
    public struct SetOffsetEvent: IEvent
    {
        public float Offset { get; }

        public SetOffsetEvent(float offset)
        {
            Offset = offset;
        }
    }
}