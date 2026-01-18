using TimeLine.Keyframe.AnimationDatas.BoxCollider.Offset;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class SpriteRendererDrawer : IComponentDrawer
    {
        private CustomInspectorDrawer _customInspectorDrawer = null;
        private KeyframeCreator _keyframeCreator = null;
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
            return component.GetType() == typeof(SpriteRendererComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component, true);
            string id = _trackObjectStorage.GetTrackObjectDataOrParentGroupBySceneObject(target).sceneObjectID;

            if (component is SpriteRendererComponent rendererComponent)
            {
                _customInspectorDrawer.CreateSpriteField(rendererComponent.Sprite);
                _customInspectorDrawer.CreateIntField(rendererComponent.OrderInLayer, null);
                // _keyframeCreater.CreateKeyframe(new YOffsetData(rendererComponent.OffsetY.Value) todo Добавить создание ключевого кадра
                _customInspectorDrawer.CreateBoolField(rendererComponent.InvertX);
                _customInspectorDrawer.CreateBoolField(rendererComponent.InvertY);
                _customInspectorDrawer.CreateColorField(rendererComponent.SpriteColor, () =>
                    _keyframeCreator.CreateKeyframe(new ColorData(rendererComponent.SpriteColor.Value), target,
                        rendererComponent.GetType().Name, rendererComponent.SpriteColor), id);
            }
        }
    }
}