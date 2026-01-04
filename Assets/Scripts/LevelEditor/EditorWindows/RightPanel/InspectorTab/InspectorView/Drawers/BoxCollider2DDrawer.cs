using TimeLine.Keyframe.AnimationDatas.BoxCollider.Offset;
using TimeLine.Keyframe.AnimationDatas.BoxCollider.Scale;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class BoxCollider2DDrawer : IComponentDrawer
    {
        private KeyframeCreator _keyframeCreator;
        private CustomInspectorDrawer _customInspectorDrawer = null;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, KeyframeCreator keyframeCreator)
        {
            _customInspectorDrawer = customInspectorDrawer;
            _keyframeCreator = keyframeCreator;
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
                _customInspectorDrawer.CreateBoolField(rendererComponent.isActive);
                
                _customInspectorDrawer.CreateFloatField(rendererComponent.OffsetX,
                    () => _keyframeCreator.CreateKeyframe(new XOffsetData(rendererComponent.OffsetX.Value), target,
                        rendererComponent.GetType().Name, rendererComponent.OffsetX));
                
                _customInspectorDrawer.CreateFloatField(rendererComponent.OffsetY, () =>
                    _keyframeCreator.CreateKeyframe(new YOffsetData(rendererComponent.OffsetY.Value), target,
                        rendererComponent.GetType().Name, rendererComponent.OffsetY));
                
                _customInspectorDrawer.AddSpace(5);
                
                _customInspectorDrawer.CreateFloatField(rendererComponent.SizeX, () =>
                    _keyframeCreator.CreateKeyframe(new XSizeData(rendererComponent.SizeX.Value), target,
                        rendererComponent.GetType().Name, rendererComponent.SizeX));
                
                _customInspectorDrawer.CreateFloatField(rendererComponent.SizeY, () =>
                    _keyframeCreator.CreateKeyframe(new YSizeData(rendererComponent.SizeY.Value), target,
                        rendererComponent.GetType().Name, rendererComponent.SizeY));
                
                _customInspectorDrawer.AddSpace(5);
                
                _customInspectorDrawer.CreateBoolField(rendererComponent.isDamageable);
                _customInspectorDrawer.CreateBoolField(rendererComponent.isObstacle);

            }
        }
    }
}