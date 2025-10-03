using EventBus;

namespace TimeLine.EventBus.Events.Bezier
{
    public class BezierSelectPointEvent : IEvent
    {
        public BezierPoint BezierPoint { get; }

        public BezierSelectPointEvent(BezierPoint bezierPoint)
        {
            BezierPoint = bezierPoint;
        }
    }
}