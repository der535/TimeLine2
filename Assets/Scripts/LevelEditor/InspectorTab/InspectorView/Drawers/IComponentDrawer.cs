using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public interface IComponentDrawer
    {
        public void Setup(CustomInspectorDrawer drawer, TrackObjectStorage trackObjectStorage, KeyframeCreator keyframeCreator);
        public bool GetComponent(Component component);
        public void Draw(Component component, GameObject target);
    }
}