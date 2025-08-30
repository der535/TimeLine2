using EventBus;
using TimeLine.Keyframe;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct RemoveTrackObjectDataEvent : IEvent
    {
        public TrackObjectData TrackObjectData { get; }

        public RemoveTrackObjectDataEvent(TrackObjectData trackObjectData)
        {
            TrackObjectData = trackObjectData;
        }
    }
}