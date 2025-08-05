using EventBus;

namespace TimeLine.EventBus.Events.Input
{
    public struct MouseScrollDeltaY: IEvent
    {
        public float Y { get; }

        public MouseScrollDeltaY(float y)
        {
            Y = y;
        }
    }
}