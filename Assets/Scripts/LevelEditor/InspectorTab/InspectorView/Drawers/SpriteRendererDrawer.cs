using System.Collections.Generic;
using TimeLine.LevelEditor.ECS.Services;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.LevelEditor.EditorWindows.SceneView.TransformTools;
using TimeLine.LevelEditor.General;
using TimeLine.LevelEditor.InspectorTab.InspectorView.Drawers;
using TimeLine.LevelEditor.SpriteLoader;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
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

        private CustomSpriteStorage CustomSpriteStorage;

        public SpriteRendererDrawer(CustomSpriteStorage customSpriteStorage)
        {
            CustomSpriteStorage = customSpriteStorage;
        }

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
                        (value) => { currentMat.mainTexture = value; }, () =>
                        {
                            _keyframeCreator.CreateKeyframe(new EntitySpriteRendererSprite(currentMat.mainTexture.name), target,
                                "Sprite", Color.white, "SpriteRenderer", ComponentNames.SpriteRenderer);
                        });


                    _customInspectorDrawer.CreateButton(() =>
                    {
                        float ppu = CustomSpriteStorage.GetPPU(currentMat.mainTexture.name);
                        PostTransformMatrix postTransformMatrix = manager.GetComponentData<PostTransformMatrix>(target);
                        float3 scale = GetScaleFromMatrix.Get(postTransformMatrix.Value);
                        scale.x = currentMat.mainTexture.width / ppu;
                        scale.y = currentMat.mainTexture.height / ppu;
                        postTransformMatrix.Value = float4x4.Scale(scale);
                        manager.SetComponentData(target, postTransformMatrix);
                    }, "Set native size");

                    _customInspectorDrawer.CreateColorField(value => currentMat.color = value, currentMat.color, () =>
                    {
                        Material currentMat = null;
                        RenderMeshArray rma = manager.GetSharedComponentManaged<RenderMeshArray>(target);
                        if (manager.HasComponent<MaterialMeshInfo>(target))
                        {
                            var meshInfo = manager.GetComponentData<MaterialMeshInfo>(target);

                            // Получаем текущий материал  
                            currentMat = rma.GetMaterial(meshInfo);
                        }

                        _keyframeCreator.CreateKeyframe(new EntitySpriteRendererColor(currentMat.color), target,
                            "Color", Color.white, "SpriteRenderer", ComponentNames.SpriteRenderer);
                    });

                    int blendMode = currentMat.GetInt("_ColorBlendMode");
                    List<string> options = new List<string>();
                    options.Add("Multiply");
                    options.Add("Add");
                    options.Add("Screen");
                    options.Add("Overlay");
                    options.Add("Darken");
                    options.Add("Lighten");
                    options.Add("SolidColor");
                    _customInspectorDrawer.CreateDropDown(blendMode, options,
                        (data) => { currentMat.SetInt("_ColorBlendMode", data); });


                    _customInspectorDrawer.CreateIntField(
                        manager.GetComponentData<LocalTransform>(target).Position.z * 100 * -1, "Order in layer",
                        (value) =>
                        {
                            LocalTransform localTransform = manager.GetComponentData<LocalTransform>(target);
                            localTransform.Position.z = value / 100 * -1;
                            manager.SetComponentData<LocalTransform>(target, localTransform);
                        }, null);
                }
            }
        }
    }
}