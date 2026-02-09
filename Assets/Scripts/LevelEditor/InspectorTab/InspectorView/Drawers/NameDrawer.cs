using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class NameDrawer : IComponentDrawer
    {
        private CustomInspectorDrawer _customInspectorDrawer;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, TrackObjectStorage trackObjectStorage,
            KeyframeCreator keyframeCreator)
        {
            _customInspectorDrawer = customInspectorDrawer;
        }

        public bool GetComponent(Component component)
        {
            return component.GetType() == typeof(NameComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component, false);

            if (component is NameComponent componentComponent)
            {
                _customInspectorDrawer.CreateStringField(componentComponent.Name);
            }
        }
    }
}