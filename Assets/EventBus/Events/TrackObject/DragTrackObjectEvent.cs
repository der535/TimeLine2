using EventBus;
using TimeLine.Keyframe;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct DragTrackObjectEvent : IEvent
    {
        public TrackObjectData Track { get; }

        public DragTrackObjectEvent(TrackObjectData track)
        {
            Track = track;
        }
    }
}