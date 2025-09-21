using EventBus;
using TimeLine.Keyframe;
using UnityEngine;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct RemoveComponentEvent : IEvent
    {
        public TrackObjectData TrackObjectData { get; }
        public Component Component { get; }

        public RemoveComponentEvent(TrackObjectData trackObjectData, Component component)
        {
            TrackObjectData = trackObjectData;
            Component = component;
        }
    }
}