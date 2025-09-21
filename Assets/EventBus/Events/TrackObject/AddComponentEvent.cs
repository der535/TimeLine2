using EventBus;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct AddComponentEvent : IEvent
    {
        public TrackObjectData TrackObjectData { get; }
        public IInitializedComponent InitializedComponent { get; }

        public AddComponentEvent(TrackObjectData trackObjectData, IInitializedComponent initializedComponent)
        {
            TrackObjectData = trackObjectData;
            InitializedComponent = initializedComponent;
        }
    }
}