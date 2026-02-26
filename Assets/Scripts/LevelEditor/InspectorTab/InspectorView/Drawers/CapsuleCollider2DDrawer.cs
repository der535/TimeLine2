using TimeLine.Keyframe.AnimationDatas.BoxCollider.Offset;
using TimeLine.Keyframe.AnimationDatas.BoxCollider.Scale;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.BoxCollider.Offset;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class CapsuleCollider2DDrawer : IComponentDrawer
    {
        private KeyframeCreator _keyframeCreator;
        private CustomInspectorDrawer _customInspectorDrawer = null;
        private TrackObjectStorage _trackObjectStorage = null;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, TrackObjectStorage trackObjectStorage,
            KeyframeCreator keyframeCreator)
        {
            _customInspectorDrawer = customInspectorDrawer;
            _keyframeCreator = keyframeCreator;
            _trackObjectStorage = trackObjectStorage;
        }

        public bool GetComponent(Component component)
        {
            return component.GetType() == typeof(CapsuleCollider2DComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component, true);
            string id = _trackObjectStorage.GetTrackObjectDataOrParentGroupBySceneObject(target).sceneObjectID;
            SceneObjectLink link = target.GetComponent<SceneObjectLink>();
            if (component is CapsuleCollider2DComponent rendererComponent)
            {
                _customInspectorDrawer.CreateBoolField(rendererComponent.isActive);

                _customInspectorDrawer.CreateFloatField(rendererComponent.OffsetX, link.trackObjectPacket,
                    (BaseParameterComponent)component, id,
                    () => _keyframeCreator.CreateKeyframe(new XOffsetData(rendererComponent.OffsetX.Value), target,
                        rendererComponent.GetType().Name, rendererComponent.OffsetX));

                _customInspectorDrawer.CreateFloatField(rendererComponent.OffsetY, link.trackObjectPacket,
                    (BaseParameterComponent)component, id, () =>
                        _keyframeCreator.CreateKeyframe(new YOffsetData(rendererComponent.OffsetY.Value), target,
                            rendererComponent.GetType().Name, rendererComponent.OffsetY));


                _customInspectorDrawer.AddSpace(5);

                _customInspectorDrawer.CreateFloatField(rendererComponent.SizeX, link.trackObjectPacket,
                    (BaseParameterComponent)component, id, () =>
                        _keyframeCreator.CreateKeyframe(new XSizeData(rendererComponent.SizeX.Value), target,
                            rendererComponent.GetType().Name, rendererComponent.SizeX));

                _customInspectorDrawer.CreateFloatField(rendererComponent.SizeY, link.trackObjectPacket,
                    (BaseParameterComponent)component, id, () =>
                        _keyframeCreator.CreateKeyframe(new YSizeData(rendererComponent.SizeY.Value), target,
                            rendererComponent.GetType().Name, rendererComponent.SizeY));

                _customInspectorDrawer.AddSpace(5);

                _customInspectorDrawer.CreateBoolField(rendererComponent.isVertical);
                _customInspectorDrawer.CreateBoolField(rendererComponent.isDamageable);
                _customInspectorDrawer.CreateBoolField(rendererComponent.isObstacle);
            }
        }
    }
}