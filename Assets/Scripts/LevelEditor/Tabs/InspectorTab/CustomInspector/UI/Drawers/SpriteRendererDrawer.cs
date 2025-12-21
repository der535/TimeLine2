using TimeLine.Keyframe.AnimationDatas.BoxCollider.Offset;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class SpriteRendererDrawer : IComponentDrawer
    {
        private CustomInspectorDrawer _customInspectorDrawer = null;
        private KeyframeCreater _keyframeCreater = null;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, KeyframeCreater keyframeCreater)
        {
            _customInspectorDrawer = customInspectorDrawer;
            _keyframeCreater = keyframeCreater;
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
                _customInspectorDrawer.CreateIntField(rendererComponent.OrderInLayer, null);
                // _keyframeCreater.CreateKeyframe(new YOffsetData(rendererComponent.OffsetY.Value) todo Добавить создание ключевого кадра
                _customInspectorDrawer.CreateBoolField(rendererComponent.InvertX);
                _customInspectorDrawer.CreateBoolField(rendererComponent.InvertY);
                _customInspectorDrawer.CreateColorField(rendererComponent.SpriteColor, () =>
                    _keyframeCreater.CreateKeyframe(new ColorData(rendererComponent.SpriteColor.Value), target,
                        rendererComponent.GetType().Name, rendererComponent.SpriteColor));
            }
        }
    }
}