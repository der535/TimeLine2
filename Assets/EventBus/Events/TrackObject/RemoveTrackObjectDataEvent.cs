using EventBus;
using TimeLine.Keyframe;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct RemoveTrackObjectDataEvent : IEvent
    {
        public TrackObjectPacket TrackObjectPacket { get; }

        public RemoveTrackObjectDataEvent(TrackObjectPacket trackObjectPacket)
        {
            TrackObjectPacket = trackObjectPacket;
        }
    }
}