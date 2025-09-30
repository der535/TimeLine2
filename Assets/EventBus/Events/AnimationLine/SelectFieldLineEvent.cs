using EventBus;

namespace TimeLine.EventBus.Events.Grid
{
    public class SelectFieldLineEvent : IEvent
    {
        public FieldLineData Data { get; }
        public bool Multiple { get; }

        public SelectFieldLineEvent(FieldLineData data, bool multiple)
        {
            Data = data;
            Multiple = multiple;
        }
    }
}
