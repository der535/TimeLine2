using EventBus;
using TimeLine.Keyframe;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct AddTrackObjectDataEvent : IEvent
    {
        public TrackObjectPacket TrackObjectPacket { get; }

        public AddTrackObjectDataEvent(TrackObjectPacket trackObjectPacket)
        {
            TrackObjectPacket = trackObjectPacket;
        }
    }
}