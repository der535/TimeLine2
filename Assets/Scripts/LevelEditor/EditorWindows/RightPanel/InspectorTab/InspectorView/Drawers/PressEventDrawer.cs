using TimeLine.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Components;
using UnityEngine;

namespace TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers
{
    public class PressEventDrawer : IComponentDrawer
    {
        private CustomInspectorDrawer _customInspectorDrawer;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, KeyframeCreator keyframeCreator)
        {
            _customInspectorDrawer = customInspectorDrawer;
        }

        public bool GetComponent(Component component)
        {
            return component.GetType() == typeof(PressEventComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component, true);

            if (component is PressEventComponent componentComponent)
            {
                _customInspectorDrawer.CreateKeyCode(componentComponent.KeyCodeParameter);
                _customInspectorDrawer.CreateVector2Field(componentComponent.prefabPerfectPosition);
                _customInspectorDrawer.CreateVector2Field(componentComponent.prefabMiddelPosition);
                _customInspectorDrawer.CreateVector2Field(componentComponent.prefabMissPosition);
                _customInspectorDrawer.CreateSelectComposition(componentComponent.prefabPerfect);
                _customInspectorDrawer.CreateSelectComposition(componentComponent.prefabMiddel);
                _customInspectorDrawer.CreateSelectComposition(componentComponent.prefabMiss);
                _customInspectorDrawer.CreateFloatField(componentComponent.perfectArea, null);
                _customInspectorDrawer.CreateFloatField(componentComponent.middleArea, null);
            }
        }
    }
}