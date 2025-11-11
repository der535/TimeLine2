using EventBus;
using UnityEngine;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct AddComponentEvent : IEvent
    {
        public TrackObjectData TrackObjectData { get; }
        public Component component { get; }

        public AddComponentEvent(TrackObjectData trackObjectData, Component component)
        {
            TrackObjectData = trackObjectData;
            this.component = component;
        }
    }
}