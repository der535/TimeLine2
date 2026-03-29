
using System.Collections.Generic;
using TimeLine.LevelEditor.General;
using TimeLine.LevelEditor.InspectorTab.InspectorView.Drawers;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class ShakeCameraDrawer : IComponentDrawer
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
            return
            (
                CheckIfComponentTypeInList.Check(component, typeof(ShakeCameraData))
            );
        }

        public void Draw(Entity target)
        {
            _customInspectorDrawer.CreateComponent(ComponentNames.ShakeCamera, target, true);

            var world = World.DefaultGameObjectInjectionWorld;
            var manager = world.EntityManager;

            // 2. Проверяем, является ли тип компонента типом RenderMeshArray
// 1. Проверяем RenderMeshArray (теперь как структуру!)
            if (CheckIfComponentTypeInList.Check(target, typeof(ShakeCameraData)))
            {
                TrackObjectPacket trackObjectPacket = _trackObjectStorage.GetTrackObjectData(target);

                // Чтобы вытащить конкретный материал, нам всё ещё нужен MaterialMeshInfo
                if (manager.HasComponent<ShakeCameraData>(target))
                {
                    ShakeCameraData shakeCameraData = manager.GetComponentData<ShakeCameraData>(target);
                    
                    _customInspectorDrawer.CreateFloatField(shakeCameraData.StrengthX, "Strength X", null, (value) =>
                    {
                        manager.SetComponentData(target, shakeCameraData);
                        shakeCameraData.StrengthX = value;
                    }, trackObjectPacket, "Strength X");
                    
                    _customInspectorDrawer.CreateFloatField(shakeCameraData.StrengthX, "Strength Y", null, (value) =>
                    {
                        manager.SetComponentData(target, shakeCameraData);
                        shakeCameraData.StrengthY = value;
                    }, trackObjectPacket, "Strength Y");

                    _customInspectorDrawer.CreateFloatField(shakeCameraData.Duration, "duraction", null, (value) =>
                    {
                        manager.SetComponentData(target, shakeCameraData);
                        shakeCameraData.Duration = value;
                    }, trackObjectPacket, "duraction");

                    _customInspectorDrawer.CreateFloatField(shakeCameraData.Vibrato, "Vibrato", null, (value) =>
                    {
                        manager.SetComponentData(target, shakeCameraData);
                        shakeCameraData.Vibrato = (int)value;
                    }, trackObjectPacket, "Vibrato");
                    
                    _customInspectorDrawer.CreateFloatField(shakeCameraData.Randomness, "Randomness", null, (value) =>
                    {
                        manager.SetComponentData(target, shakeCameraData);
                        shakeCameraData.Randomness = value;
                    }, trackObjectPacket, "Randomness");
                }
            }
        }
    }
}