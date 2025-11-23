using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class SpriteRendererDrawer : IComponentDrawer
    {
        private CustomInspectorDrawer _customInspectorDrawer = null;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, KeyframeCreater keyframeCreater)
        {
            _customInspectorDrawer = customInspectorDrawer;
        }

        public bool GetComponent(Component component)
        {
            return component.GetType() == typeof(SpriteRendererComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component, true);

            if (component is SpriteRendererComponent rendererComponent)
            {
                _customInspectorDrawer.CreateSpriteField(rendererComponent.Sprite);
                _customInspectorDrawer.CreateBoolField(rendererComponent.InvertX);
                _customInspectorDrawer.CreateBoolField(rendererComponent.InvertY);
                _customInspectorDrawer.CreateColorField(rendererComponent.SpriteColor);
            }
        }
    }
}