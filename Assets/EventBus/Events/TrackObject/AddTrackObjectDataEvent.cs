using EventBus;
using TimeLine.Keyframe;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct AddTrackObjectDataEvent : IEvent
    {
        public TrackObjectData TrackObjectData { get; }

        public AddTrackObjectDataEvent(TrackObjectData trackObjectData)
        {
            TrackObjectData = trackObjectData;
        }
    }
}