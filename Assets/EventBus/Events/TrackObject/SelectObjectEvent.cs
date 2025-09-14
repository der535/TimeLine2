using System.Collections.Generic;
using EventBus;

namespace TimeLine.EventBus.Events.TrackObject
{
    public class SelectObjectEvent: IEvent
    {
        public List<TrackObjectData> Tracks { get; }

        public SelectObjectEvent(List<TrackObjectData> tracks)
        {
            Tracks = tracks;
        }
    }
}