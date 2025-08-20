using EventBus;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct SelectKeyframeEvent : IEvent
    {
        public KeyframeObjectData Keyframe { get; }

        public SelectKeyframeEvent(KeyframeObjectData keyframe)
        {
            Keyframe = keyframe;
        }
    }
}