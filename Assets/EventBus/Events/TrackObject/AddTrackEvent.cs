using EventBus;
using TimeLine.Keyframe;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct AddTrackEvent : IEvent
    {
        public Track Track { get; }

        public AddTrackEvent(Track track)
        {
            Track = track;
        }
    }
}