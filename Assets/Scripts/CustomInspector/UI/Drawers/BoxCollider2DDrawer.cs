using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class BoxCollider2DDrawer : IComponentDrawer
    {
        private CustomInspectorDrawer _customInspectorDrawer = null;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, KeyframeCreater keyframeCreater)
        {
            _customInspectorDrawer = customInspectorDrawer;
        }

        public bool GetComponent(Component component)
        {
            return component.GetType() == typeof(BoxCollider2DComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component, true);

            if (component is BoxCollider2DComponent rendererComponent)
            {
                _customInspectorDrawer.CreateFloatField(rendererComponent.OffsetX, null);
                _customInspectorDrawer.CreateFloatField(rendererComponent.OffsetY, null);
                _customInspectorDrawer.AddSpace(5);
                _customInspectorDrawer.CreateFloatField(rendererComponent.SizeX, null);
                _customInspectorDrawer.CreateFloatField(rendererComponent.SizeY, null);
            }
        }
    }
}