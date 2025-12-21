using TimeLine.Keyframe.AnimationDatas.BoxCollider.Offset;
using TimeLine.Keyframe.AnimationDatas.BoxCollider.Scale;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class CircleCollider2DDrawer : IComponentDrawer
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
            return component.GetType() == typeof(CircleCollider2DComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component, true);

            if (component is CircleCollider2DComponent rendererComponent)
            {
                _customInspectorDrawer.CreateBoolField(rendererComponent.isActive);
                
                _customInspectorDrawer.CreateFloatField(rendererComponent.OffsetX,
                    () => _keyframeCreater.CreateKeyframe(new XOffsetData(rendererComponent.OffsetX.Value), target,
                        rendererComponent.GetType().Name, rendererComponent.OffsetX));
                
                _customInspectorDrawer.CreateFloatField(rendererComponent.OffsetY, () =>
                    _keyframeCreater.CreateKeyframe(new YOffsetData(rendererComponent.OffsetY.Value), target,
                        rendererComponent.GetType().Name, rendererComponent.OffsetY));
                
                
                _customInspectorDrawer.AddSpace(5);
                
                _customInspectorDrawer.CreateFloatField(rendererComponent.Radius, () =>
                    _keyframeCreater.CreateKeyframe(new XSizeData(rendererComponent.Radius.Value), target,
                        rendererComponent.GetType().Name, rendererComponent.Radius));
                
                _customInspectorDrawer.AddSpace(5);
                
                _customInspectorDrawer.CreateBoolField(rendererComponent.isDamageable);
                _customInspectorDrawer.CreateBoolField(rendererComponent.isObstacle);

            }
        }
    }
}