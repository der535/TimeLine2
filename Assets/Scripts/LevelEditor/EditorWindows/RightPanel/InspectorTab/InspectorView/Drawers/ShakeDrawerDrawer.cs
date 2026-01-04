using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class ShakeDrawer : IComponentDrawer
    {
        private CustomInspectorDrawer _customInspectorDrawer = null;
        private KeyframeCreator _keyframeCreator = null;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, KeyframeCreator keyframeCreator)
        {
            _customInspectorDrawer = customInspectorDrawer;
            _keyframeCreator = keyframeCreator;
        }

        public bool GetComponent(Component component)
        {
            return component.GetType() == typeof(ShakeComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component, true);

            if (component is ShakeComponent shakeComponent)
            {
                _customInspectorDrawer.CreateVector2Field(shakeComponent.ShakeStrength);
                _customInspectorDrawer.CreateFloatField(shakeComponent.Duration, null);
                _customInspectorDrawer.CreateIntField(shakeComponent.Vibrato, null);
                _customInspectorDrawer.CreateFloatField(shakeComponent.Randomness, null);
            }
        }
    }
}