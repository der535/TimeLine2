using EventBus;
using TimeLine.Keyframe;
using UnityEngine;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct RemoveComponentEvent : IEvent
    {
        public TrackObjectPacket TrackObjectPacket { get; }
        public Component Component { get; }

        public RemoveComponentEvent(TrackObjectPacket trackObjectPacket, Component component)
        {
            TrackObjectPacket = trackObjectPacket;
            Component = component;
        }
    }
}