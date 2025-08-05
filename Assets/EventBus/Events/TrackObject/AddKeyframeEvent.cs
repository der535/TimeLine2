using EventBus;
using TimeLine.Keyframe;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct AddKeyframeEvent : IEvent
    {
        public Keyframe.Keyframe Keyframe { get; }

        public AddKeyframeEvent(Keyframe.Keyframe keyframe)
        {
            Keyframe = keyframe;
        }
    }
}