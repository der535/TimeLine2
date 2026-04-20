using System.Collections.Generic;
using TimeLine.LevelEditor.EditorWindows.SceneView.TransformTools;
using TimeLine.LevelEditor.General;
using TimeLine.LevelEditor.InspectorTab.InspectorView.Drawers;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class SunBurstMaterialDrawer : IComponentDrawer
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
                CheckIfComponentTypeInList.Check(component, typeof(SunBurstMaterialData))
            );
        }

        public void Draw(Entity target)
        {
            _customInspectorDrawer.CreateComponent(ComponentNames.SunBurstMaterial, target, true);

            var world = World.DefaultGameObjectInjectionWorld;
            var manager = world.EntityManager;

            // 2. Проверяем, является ли тип компонента типом RenderMeshArray
// 1. Проверяем RenderMeshArray (теперь как структуру!)
            if (CheckIfComponentTypeInList.Check(target, new List<ComponentType>()
                {
                    typeof(RenderMeshArray),
                    typeof(SunBurstMaterialData),
                }))
            {
                // Для ISharedComponentData используем этот метод:
                RenderMeshArray rma = manager.GetSharedComponentManaged<RenderMeshArray>(target);
                TrackObjectPacket trackObjectPacket = _trackObjectStorage.GetTrackObjectData(target);

                // Чтобы вытащить конкретный материал, нам всё ещё нужен MaterialMeshInfo
                if (manager.HasComponent<MaterialMeshInfo>(target))
                {
                    var meshInfo = manager.GetComponentData<MaterialMeshInfo>(target);

                    // Получаем текущий материал
                    Material currentMat = rma.GetMaterial(meshInfo);


                    _customInspectorDrawer.CreateColorField(value =>
                        {
                            SunBurstMaterialData data = manager.GetComponentData<SunBurstMaterialData>(target);
                            currentMat.SetColor("_BaseColor", value);
                            data.Color1 = value;
                            manager.SetComponentData(target,data);
                        },
                        currentMat.GetColor("_BaseColor"), null);
                    _customInspectorDrawer.CreateColorField(value =>
                        {
                            SunBurstMaterialData data = manager.GetComponentData<SunBurstMaterialData>(target);
                            currentMat.SetColor("_LineColor", value);
                            data.Color2 = value;
                            manager.SetComponentData(target,data);
                        },
                        currentMat.GetColor("_LineColor"), null);
                    
                    _customInspectorDrawer.CreateFloatField(currentMat.GetFloat("_LineCount"), "Line count", null,
                        (value) =>
                        {
                            SunBurstMaterialData data = manager.GetComponentData<SunBurstMaterialData>(target);
                            currentMat.SetFloat("_LineCount", value);
                            data.LineCount = (int)value;
                            manager.SetComponentData(target,data);
                        },trackObjectPacket, "LineCount");
                    
                    _customInspectorDrawer.CreateFloatField(currentMat.GetFloat("_RotationOffset"), "_RotationOffset", null,
                        (value) =>
                        {
                            SunBurstMaterialData data = manager.GetComponentData<SunBurstMaterialData>(target);
                            currentMat.SetFloat("_RotationOffset", value);
                            data.Offset = value;
                            manager.SetComponentData(target,data);
                        },trackObjectPacket, "_RotationOffset");
                    
                    _customInspectorDrawer.CreateFloatField(currentMat.GetFloat("_Twist"), "_Twist", null,
                        (value) =>
                        {
                            SunBurstMaterialData data = manager.GetComponentData<SunBurstMaterialData>(target);
                            currentMat.SetFloat("_Twist", value);
                            data.TwistFactor = value;
                            manager.SetComponentData(target,data);
                        },trackObjectPacket, "_Twist");


                    _customInspectorDrawer.CreateIntField(manager.GetComponentData<LocalTransform>(target).Position.z,
                        "Order in layer",
                        (value) =>
                        {
                            LocalTransform localTransform = manager.GetComponentData<LocalTransform>(target);
                            localTransform.Position.z = value / 100;
                            manager.SetComponentData<LocalTransform>(target, localTransform);
                        }, null);
                }
            }
        }
    }
}