using EventBus;

namespace TimeLine.EventBus.Events.ValueEditor
{
    public struct SelectNodeConnectionEvent : IEvent
    {
        public NodeConnection Node { get; }

        public SelectNodeConnectionEvent(NodeConnection node)
        {
            Node = node;
        }
    }
}