using EventBus;
using TimeLine.Keyframe;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct AddComponentObjectDataEvent : IEvent
    {
        public TrackObjectData TrackObjectData { get; }
        public IInitializedComponent InitializedComponent { get; }

        public AddComponentObjectDataEvent(TrackObjectData trackObjectData, IInitializedComponent initializedComponent)
        {
            TrackObjectData = trackObjectData;
            InitializedComponent = initializedComponent;
        }
    }
}