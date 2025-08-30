using EventBus;

namespace TimeLine.EventBus.Events.TrackObject
{
    public class SelectObjectEvent: IEvent
    {
        public TrackObjectData Track { get; }

        public SelectObjectEvent(TrackObjectData track)
        {
            Track = track;
        }
    }
}