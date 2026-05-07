using System.Collections.Generic;
using EventBus;
using TimeLine.LevelEditor.EditorWindows.SceneView.TransformTools;
using TimeLine.LevelEditor.General;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using Unity.Entities;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class CapsuleCollider2DDrawer : IComponentDrawer
    {
        private KeyframeCreator _keyframeCreator;
        private CustomInspectorDrawer _customInspectorDrawer = null;
        private TrackObjectStorage _trackObjectStorage = null;
        private GameEventBus _gameEventBus;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, TrackObjectStorage trackObjectStorage,
            KeyframeCreator keyframeCreator, ToolsController toolsController, TimeLineRecorder timeLineRecorder, GameEventBus gameEventBus)
        {
            _customInspectorDrawer = customInspectorDrawer;
            _keyframeCreator = keyframeCreator;
            _trackObjectStorage = trackObjectStorage;
            _gameEventBus = gameEventBus;
        }

     

        public bool GetComponent(List<ComponentType> component)
        {
            return false;
        }

        public void Draw(Entity target)
        {
            throw new System.NotImplementedException();
        }
    }
}