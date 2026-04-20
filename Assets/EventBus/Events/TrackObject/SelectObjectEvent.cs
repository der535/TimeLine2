using System.Collections.Generic;
using EventBus;

namespace TimeLine.EventBus.Events.TrackObject
{
    public class SelectObjectEvent : IEvent
    {
        public readonly bool UpdateVisual;
        public List<TrackObjectPacket> Tracks { get; }

        public SelectObjectEvent(List<TrackObjectPacket> tracks, bool updateVisual)
        {
            UpdateVisual = updateVisual;
            Tracks = tracks;
        }
    }
}