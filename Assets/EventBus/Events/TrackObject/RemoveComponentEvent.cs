using EventBus;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct RemoveComponentEvent : IEvent
    {
        public TrackObjectPacket TrackObjectPacket { get; }
        public ComponentNames Component { get; }

        public RemoveComponentEvent(TrackObjectPacket trackObjectPacket, ComponentNames component)
        {
            TrackObjectPacket = trackObjectPacket;
            Component = component;
        }
    }
}