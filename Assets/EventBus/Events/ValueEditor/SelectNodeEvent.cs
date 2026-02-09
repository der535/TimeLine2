using EventBus;
using TimeLine.LevelEditor.ValueEditor;

namespace TimeLine.EventBus.Events.ValueEditor
{
    public struct SelectNodeEvent : IEvent
    {
        public Node Node { get; }

        public SelectNodeEvent(Node node)
        {
            Node = node;
        }
    }
}