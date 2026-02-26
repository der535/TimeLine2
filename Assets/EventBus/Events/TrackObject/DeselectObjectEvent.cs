using System.Collections.Generic;
using EventBus;

namespace TimeLine.EventBus.Events.TrackObject
{
    public class DeselectObjectEvent: IEvent
    {
        public TrackObjectPacket DeselectedObject { get; }
        public List<TrackObjectPacket> SelectedObjects { get; }

        public DeselectObjectEvent(TrackObjectPacket deselectedObject, List<TrackObjectPacket> selectedObjects)
        {
            DeselectedObject = deselectedObject;
            SelectedObjects = selectedObjects;
        }
    }
}