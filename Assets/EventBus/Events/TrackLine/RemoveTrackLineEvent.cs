using EventBus;

namespace TimeLine.EventBus.Events.TimeLine
{
    public struct RemoveTrackLineEvent : IEvent
    {
        public TrackLine TrackLine { get; }

        public RemoveTrackLineEvent(TrackLine trackLine)
        {
            TrackLine = trackLine;
        }
    }
}