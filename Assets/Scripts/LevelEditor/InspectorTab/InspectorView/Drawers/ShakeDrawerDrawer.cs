using System.Collections.Generic;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.EditorWindows.SceneView.TransformTools;
using TimeLine.LevelEditor.General;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class ShakeDrawer : IComponentDrawer
    {
        private CustomInspectorDrawer _customInspectorDrawer = null;
        private KeyframeCreator _keyframeCreator = null;
        private TrackObjectStorage _trackObjectStorage = null;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, TrackObjectStorage trackObjectStorage,
            KeyframeCreator keyframeCreator, ToolsController toolsController, TimeLineRecorder timeLineRecorder)
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
            // _customInspectorDrawer.CreateComponent(component, true);
            string id = _trackObjectStorage.GetTrackObjectDataOrParentGroupBySceneObject(target).sceneObjectID;
            SceneObjectLink link = target.GetComponent<SceneObjectLink>();
            if (component is ShakeComponent shakeComponent)
            {
                _customInspectorDrawer.CreateVector2Field(shakeComponent.ShakeStrength);
                _customInspectorDrawer.CreateFloatField(shakeComponent.Duration,                    link.trackObjectPacket,
                    (BaseParameterComponent)component, id,null, "test");
                _customInspectorDrawer.CreateIntField(shakeComponent.Vibrato, null);
                _customInspectorDrawer.CreateFloatField(shakeComponent.Randomness,                    link.trackObjectPacket,
                    (BaseParameterComponent)component, id,null, "test");
            }
        }


        
        public bool GetComponent(List<ComponentType> component)
        {
            return false;
        }

        public void Draw( Entity target)
        {
            throw new System.NotImplementedException();
        }
    }
}