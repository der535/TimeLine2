using TimeLine.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using UnityEngine;

namespace TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers
{
    public class EdgeCollider2DDrawer : IComponentDrawer
    {
        private KeyframeCreator _keyframeCreator;
        private CustomInspectorDrawer _customInspectorDrawer = null;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, TrackObjectStorage trackObjectStorage,
            KeyframeCreator keyframeCreator)
        {
            _customInspectorDrawer = customInspectorDrawer;
            _keyframeCreator = keyframeCreator;
        }

        public bool GetComponent(Component component)
        {
            return component.GetType() == typeof(EdgeCollider2DComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component, true);
            SceneObjectLink link = target.GetComponent<SceneObjectLink>();
            if (component is EdgeCollider2DComponent rendererComponent)
            {
                _customInspectorDrawer.CreateEditColliderButton();
                _customInspectorDrawer.CreateBoolField(rendererComponent.isActive);
                _customInspectorDrawer.CreateFloatField(rendererComponent.edgeRadius,                     link.trackObjectData,
                    (BaseParameterComponent)component,"0", null);
                
                _customInspectorDrawer.AddSpace(5);
                
                _customInspectorDrawer.CreateBoolField(rendererComponent.isDamageable);
                _customInspectorDrawer.CreateBoolField(rendererComponent.isObstacle);
            }
        }
    }
}