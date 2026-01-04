using System.Collections.Generic;
using EventBus;

namespace TimeLine.EventBus.Events.TrackObject
{
    public class DeselectObjectEvent: IEvent
    {
        public TrackObjectData DeselectedObject { get; }
        public List<TrackObjectData> SelectedObjects { get; }

        public DeselectObjectEvent(TrackObjectData deselectedObject, List<TrackObjectData> selectedObjects)
        {
            DeselectedObject = deselectedObject;
            SelectedObjects = selectedObjects;
        }
    }
}