using EventBus;
using TimeLine.Keyframe;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct DragTrackObjectEvent : IEvent
    {
        public TrackObjectPacket Track { get; }

        public DragTrackObjectEvent(TrackObjectPacket track)
        {
            Track = track;
        }
    }
}