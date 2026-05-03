using EventBus;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using UnityEngine;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct RemoveComponentEvent : IEvent
    {
        public TrackObjectPacket TrackObjectPacket { get; }
        public ComponentNames Component { get; }

        public RemoveComponentEvent(TrackObjectPacket trackObjectPacket, ComponentNames component)
        {
            Debug.Log("remove");
            TrackObjectPacket = trackObjectPacket;
            Component = component;
        }
    }
}