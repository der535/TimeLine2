using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class ShakeDrawer : IComponentDrawer
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
            return component.GetType() == typeof(ShakeComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component, true);
            string id = _trackObjectStorage.GetTrackObjectDataOrParentGroupBySceneObject(target).sceneObjectID;
            SceneObjectLink link = target.GetComponent<SceneObjectLink>();
            if (component is ShakeComponent shakeComponent)
            {
                _customInspectorDrawer.CreateVector2Field(shakeComponent.ShakeStrength);
                _customInspectorDrawer.CreateFloatField(shakeComponent.Duration,                    link.trackObjectPacket,
                    (BaseParameterComponent)component, id,null);
                _customInspectorDrawer.CreateIntField(shakeComponent.Vibrato, null);
                _customInspectorDrawer.CreateFloatField(shakeComponent.Randomness,                    link.trackObjectPacket,
                    (BaseParameterComponent)component, id,null);
            }
        }
    }
}