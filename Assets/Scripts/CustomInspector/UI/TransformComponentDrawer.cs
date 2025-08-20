using TimeLine.CustomInspector.UI;
using TimeLine.Keyframe.AnimationDatas.TransformComponent;
using UnityEngine;

namespace TimeLine
{
    public class TransformComponentDrawer : IComponentDrawer
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
            return component.GetType() == typeof(TransformComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component.GetType().Name);

            if (component is TransformComponent componentComponent)
            {
                _customInspectorDrawer.CreateFloatField(componentComponent.XPosition,
                    () => _keyframeCreater.CreateKeyframe(new XPositionData(componentComponent.XPosition.Value), target,
                        componentComponent.name, componentComponent.XPosition.Name));
                
                _customInspectorDrawer.CreateFloatField(componentComponent.YPosition,
                    () => _keyframeCreater.CreateKeyframe(new YPositionData(componentComponent.YPosition.Value), target,
                        componentComponent.name, componentComponent.YPosition.Name));

                _customInspectorDrawer.CreateFloatField(componentComponent.XRotation,
                    () => _keyframeCreater.CreateKeyframe(new XRotationData(componentComponent.XRotation.Value), target,
                        componentComponent.name, componentComponent.XRotation.Name));
                
                _customInspectorDrawer.CreateFloatField(componentComponent.YRotation,
                    () => _keyframeCreater.CreateKeyframe(new YRotationData(componentComponent.YRotation.Value), target,
                        componentComponent.name, componentComponent.YRotation.Name));
                
                _customInspectorDrawer.CreateFloatField(componentComponent.ZRotation,
                    () => _keyframeCreater.CreateKeyframe(new ZRotationData(componentComponent.ZRotation.Value), target,
                        componentComponent.name, componentComponent.ZRotation.Name));
                
                _customInspectorDrawer.CreateFloatField(componentComponent.XScale,
                    () => _keyframeCreater.CreateKeyframe(new XScaleData(componentComponent.XScale.Value), target,
                        componentComponent.name, componentComponent.XScale.Name));
                
                _customInspectorDrawer.CreateFloatField(componentComponent.YScale,
                    () => _keyframeCreater.CreateKeyframe(new YScaleData(componentComponent.YScale.Value), target,
                        componentComponent.name, componentComponent.YScale.Name));
            }
        }
    }
}