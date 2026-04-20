using System.Collections.Generic;
using TimeLine.LevelEditor.EditorWindows.SceneView.TransformTools;
using TimeLine.LevelEditor.General;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using Unity.Entities;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class RadialSunburstDrawer : IComponentDrawer
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