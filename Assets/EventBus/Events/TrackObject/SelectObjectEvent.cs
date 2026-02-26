using System.Collections.Generic;
using EventBus;

namespace TimeLine.EventBus.Events.TrackObject
{
    public class SelectObjectEvent: IEvent
    {
        public List<TrackObjectPacket> Tracks { get; }

        public SelectObjectEvent(List<TrackObjectPacket> tracks)
        {
            Tracks = tracks;
        }
    }
}