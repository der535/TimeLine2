using EventBus;

namespace TimeLine.EventBus.Events.Grid
{
    public class GridPositionEvent : IEvent
    {
        public float StepSize { get; }

        public GridPositionEvent(float stepSize)
        {
            StepSize = stepSize;
        }
    }
}
