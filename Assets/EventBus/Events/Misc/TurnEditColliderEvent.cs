using EventBus;

namespace TimeLine.EventBus.Events.Misc
{
    public class TurnEditColliderEvent : IEvent
    {
        public bool IsEditing { get; }

        public TurnEditColliderEvent(bool isEditing)
        {
            IsEditing = isEditing;
        }
    }
}