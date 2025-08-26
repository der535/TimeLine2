using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class DynamicTransformDrawer : IComponentDrawer
    {
        private CustomInspectorDrawer _customInspectorDrawer;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, KeyframeCreater keyframeCreater)
        {
            _customInspectorDrawer = customInspectorDrawer;
        }

        public bool GetComponent(Component component)
        {
            return component.GetType() == typeof(DynamicTransformСomponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component.GetType().Name);

            if (component is DynamicTransformСomponent componentComponent)
            {
                _customInspectorDrawer.CreateBoolField(componentComponent.ComponentActive);
                _customInspectorDrawer.AddSpace(60);
                _customInspectorDrawer.CreateVector2Field(componentComponent.DynamicXPosition);
                _customInspectorDrawer.CreateBoolField(componentComponent.DynamicXPositionActive);
                _customInspectorDrawer.CreateVector2Field(componentComponent.DynamicYPosition);
                _customInspectorDrawer.CreateBoolField(componentComponent.DynamicYPositionActive);
                _customInspectorDrawer.AddSpace(30);
                _customInspectorDrawer.CreateVector2Field(componentComponent.DynamicXRotation);
                _customInspectorDrawer.CreateBoolField(componentComponent.DynamicXRotationActive);
                _customInspectorDrawer.CreateVector2Field(componentComponent.DynamicYRotation);
                _customInspectorDrawer.CreateBoolField(componentComponent.DynamicYRotationActive);
                _customInspectorDrawer.CreateVector2Field(componentComponent.DynamicZRotation);
                _customInspectorDrawer.CreateBoolField(componentComponent.DynamicZRotationActive);
                _customInspectorDrawer.AddSpace(30);
                _customInspectorDrawer.CreateVector2Field(componentComponent.DynamicXScale);
                _customInspectorDrawer.CreateBoolField(componentComponent.DynamicXScaleActive);
                _customInspectorDrawer.CreateVector2Field(componentComponent.DynamicYScale);
                _customInspectorDrawer.CreateBoolField(componentComponent.DynamicYScaleActive);
            }
        }
    }
}