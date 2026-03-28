using EventBus;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;

namespace TimeLine.EventBus.Events.Misc
{
    public class GetParameterEvent : IEvent
    {
        public MapParameterComponen _map { get; }
        public TrackObjectPacket _trackObjectPacket { get; }

        public GetParameterEvent(MapParameterComponen map, TrackObjectPacket trackObjectPacket)
        {
            _map = map;
            _trackObjectPacket = trackObjectPacket;
        }
    }
}