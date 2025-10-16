using TimeLine.Keyframe.AnimationDatas.TransformComponent;
using TimeLine.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.Keyframe.AnimationDatas.TransformComponent.Rotation;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
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
            Debug.Log(component);
            return component.GetType() == typeof(TransformComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component, false);

            if (component is TransformComponent componentComponent)
            {
                _customInspectorDrawer.CreateFloatField(componentComponent.XPosition,
                    () =>
                    {
                        _keyframeCreater.CreateKeyframe(new XPositionData(componentComponent.XPosition.Value), target,
                            componentComponent.GetType().Name, componentComponent.XPosition);
                    });
                
                _customInspectorDrawer.CreateBoolField(componentComponent.XPositionActive);
                
                _customInspectorDrawer.CreateFloatField(componentComponent.YPosition,
                    () => _keyframeCreater.CreateKeyframe(new YPositionData(componentComponent.YPosition.Value), target,
                        componentComponent.GetType().Name, componentComponent.YPosition));
                
                _customInspectorDrawer.CreateBoolField(componentComponent.YPositionActive);

                _customInspectorDrawer.AddSpace(30);
                _customInspectorDrawer.CreateFloatField(componentComponent.XRotation,
                    () => _keyframeCreater.CreateKeyframe(new XRotationData(componentComponent.XRotation.Value), target,
                        componentComponent.GetType().Name, componentComponent.XRotation));
                
                _customInspectorDrawer.CreateBoolField(componentComponent.XRotationActive);
                
                _customInspectorDrawer.CreateFloatField(componentComponent.YRotation,
                    () => _keyframeCreater.CreateKeyframe(new YRotationData(componentComponent.YRotation.Value), target,
                        componentComponent.GetType().Name, componentComponent.YRotation));
                
                _customInspectorDrawer.CreateBoolField(componentComponent.YRotationActive);
                
                _customInspectorDrawer.CreateFloatField(componentComponent.ZRotation,
                    () => _keyframeCreater.CreateKeyframe(new ZRotationData(componentComponent.ZRotation.Value), target,
                        componentComponent.GetType().Name, componentComponent.ZRotation));
                
                _customInspectorDrawer.CreateBoolField(componentComponent.ZRotationActive);
                _customInspectorDrawer.AddSpace(30);
                _customInspectorDrawer.CreateFloatField(componentComponent.XScale,
                    () => _keyframeCreater.CreateKeyframe(new XScaleData(componentComponent.XScale.Value), target,
                        componentComponent.GetType().Name, componentComponent.XScale));
                
                _customInspectorDrawer.CreateBoolField(componentComponent.XScaleActive);
                
                _customInspectorDrawer.CreateFloatField(componentComponent.YScale,
                    () => _keyframeCreater.CreateKeyframe(new YScaleData(componentComponent.YScale.Value), target,
                        componentComponent.GetType().Name, componentComponent.YScale));
                
                _customInspectorDrawer.CreateBoolField(componentComponent.YScaleActive);
            }
        }
    }
}