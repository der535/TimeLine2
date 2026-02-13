using EventBus;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;

namespace TimeLine.EventBus.Events.Misc
{
    public class GetParameterEvent : IEvent
    {
        public (InspectableParameter, MapParameterComponen) Parameter { get; }

        public GetParameterEvent((InspectableParameter, MapParameterComponen) parameter)
        {
            Parameter = parameter;
        }
    }
}