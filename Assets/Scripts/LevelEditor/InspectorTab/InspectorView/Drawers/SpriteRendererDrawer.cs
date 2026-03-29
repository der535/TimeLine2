using System.Collections.Generic;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Position;
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
    public class SpriteRendererDrawer : IComponentDrawer
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
                CheckIfComponentTypeInList.Check(component, typeof(SpriteRendererTag))
            );
        }

        public void Draw(Entity target)
        {
            _customInspectorDrawer.CreateComponent(ComponentNames.SpriteRenderer, target, true);

            var world = World.DefaultGameObjectInjectionWorld;
            var manager = world.EntityManager;

            // 2. Проверяем, является ли тип компонента типом RenderMeshArray
// 1. Проверяем RenderMeshArray (теперь как структуру!)
            if (CheckIfComponentTypeInList.Check(target, typeof(RenderMeshArray)))
            {
                // Для ISharedComponentData используем этот метод:
                RenderMeshArray rma = manager.GetSharedComponentManaged<RenderMeshArray>(target);

                // Чтобы вытащить конкретный материал, нам всё ещё нужен MaterialMeshInfo
                if (manager.HasComponent<MaterialMeshInfo>(target))
                {
                    var meshInfo = manager.GetComponentData<MaterialMeshInfo>(target);

                    // Получаем текущий материал
                    Material currentMat = rma.GetMaterial(meshInfo);
                    
                    _customInspectorDrawer.CreateSpriteField(currentMat.mainTexture.name,
                        (value) =>
                        {
                            Debug.Log(value.name);
                            currentMat.mainTexture = value;
                        });
                    
                    
                                            

                    
                    _customInspectorDrawer.CreateColorField(value => currentMat.color = value,  currentMat.color, () =>
                    {
                        Material currentMat = null;  
                        RenderMeshArray rma = manager.GetSharedComponentManaged<RenderMeshArray>(target);  
                        if (manager.HasComponent<MaterialMeshInfo>(target))  
                        {  
                            var meshInfo = manager.GetComponentData<MaterialMeshInfo>(target);  
  
                            // Получаем текущий материал  
                            currentMat = rma.GetMaterial(meshInfo);  
                        }
                        _keyframeCreator.CreateKeyframe(new EntitySpriteRendererColor(currentMat.color), target, "Color", Color.white, "SpriteRenderer");
                    }  );
                    
                    
                    _customInspectorDrawer.CreateIntField(manager.GetComponentData<LocalTransform>(target).Position.z, "Order in layer",
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