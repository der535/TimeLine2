using EventBus;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;

namespace TimeLine.EventBus.Events.Misc
{
    public class TrackObjectResizing : IEvent
    {
        public bool IsResizing { get; }

        public TrackObjectResizing(bool isResizing)
        {
            IsResizing = isResizing;
        }
    }
}