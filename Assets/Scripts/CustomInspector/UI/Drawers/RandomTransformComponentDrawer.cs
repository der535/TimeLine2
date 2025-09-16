using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class RandomTransformComponentDrawer : IComponentDrawer
    {
        private CustomInspectorDrawer _customInspectorDrawer = null;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, KeyframeCreater keyframeCreater)
        {
            _customInspectorDrawer = customInspectorDrawer;
        }

        public bool GetComponent(Component component)
        {
            return component.GetType() == typeof(RandomTransformComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component);

            if (component is RandomTransformComponent componentComponent)
            {
                _customInspectorDrawer.CreateBoolField(componentComponent.ComponentActive);
                _customInspectorDrawer.AddSpace(60);
                _customInspectorDrawer.CreateVector2Field(componentComponent.XRandomPosition);
                _customInspectorDrawer.CreateBoolField(componentComponent.XRandomPositionActive);
                _customInspectorDrawer.CreateVector2Field(componentComponent.YRandomPosition);
                _customInspectorDrawer.CreateBoolField(componentComponent.YRandomPositionActive);
                _customInspectorDrawer.AddSpace(30);
                _customInspectorDrawer.CreateVector2Field(componentComponent.XRandomRotation);
                _customInspectorDrawer.CreateBoolField(componentComponent.XRandomRotationActive);
                _customInspectorDrawer.CreateVector2Field(componentComponent.YRandomRotation);
                _customInspectorDrawer.CreateBoolField(componentComponent.YRandomRotationActive);
                _customInspectorDrawer.CreateVector2Field(componentComponent.ZRandomRotation);
                _customInspectorDrawer.CreateBoolField(componentComponent.ZRandomRotationActive);
                _customInspectorDrawer.AddSpace(30);
                _customInspectorDrawer.CreateVector2Field(componentComponent.XRandomScale);
                _customInspectorDrawer.CreateBoolField(componentComponent.XRandomScaleActive);
                _customInspectorDrawer.CreateVector2Field(componentComponent.YRandomScale);
                _customInspectorDrawer.CreateBoolField(componentComponent.YRandomScaleActive);
            }
        }
    }
}