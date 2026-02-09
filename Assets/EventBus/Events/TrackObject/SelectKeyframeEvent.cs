using EventBus;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct SelectKeyframeEvent : IEvent
    {
        public Keyframe.Keyframe Keyframe { get; }

        public SelectKeyframeEvent(Keyframe.Keyframe keyframe)
        {
            Keyframe = keyframe;
        }
    }
}