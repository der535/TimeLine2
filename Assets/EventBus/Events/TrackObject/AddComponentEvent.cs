using EventBus;
using UnityEngine;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct AddComponentEvent : IEvent
    {
        public TrackObjectPacket TrackObjectPacket { get; }
        public Component component { get; }

        public AddComponentEvent(TrackObjectPacket trackObjectPacket, Component component)
        {
            TrackObjectPacket = trackObjectPacket;
            this.component = component;
        }
    }
}