using EventBus;

namespace TimeLine.EventBus.Events.Bezier
{
    public class DragTangentEvent : IEvent
    {
        public bool IsDraging { get; }

        public DragTangentEvent( bool isDraging)
        {
            IsDraging = isDraging;
        }
    }
}