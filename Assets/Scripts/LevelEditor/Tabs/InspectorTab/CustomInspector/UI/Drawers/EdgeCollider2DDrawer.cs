using TimeLine.Keyframe.AnimationDatas.BoxCollider.Offset;
using TimeLine.Keyframe.AnimationDatas.BoxCollider.Scale;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class EdgeCollider2DDrawer : IComponentDrawer
    {
        private KeyframeCreater _keyframeCreater;
        private CustomInspectorDrawer _customInspectorDrawer = null;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, KeyframeCreater keyframeCreater)
        {
            _customInspectorDrawer = customInspectorDrawer;
            _keyframeCreater = keyframeCreater;
        }

        public bool GetComponent(Component component)
        {
            return component.GetType() == typeof(EdgeCollider2DComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component, true);

            if (component is EdgeCollider2DComponent rendererComponent)
            {
                _customInspectorDrawer.CreateBoolField(rendererComponent.isActive);
                
                _customInspectorDrawer.AddSpace(5);
                
                _customInspectorDrawer.CreateBoolField(rendererComponent.isDamageable);
                _customInspectorDrawer.CreateBoolField(rendererComponent.isObstacle);
            }
        }
    }
}