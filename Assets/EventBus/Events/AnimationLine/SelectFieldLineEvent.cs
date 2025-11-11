using EventBus;

namespace TimeLine.EventBus.Events.Grid
{
    public class SelectFieldLineEvent : IEvent
    {
        public FieldLineData Data { get; }

        public SelectFieldLineEvent(FieldLineData data)
        {
            Data = data;
        }
    }
}
