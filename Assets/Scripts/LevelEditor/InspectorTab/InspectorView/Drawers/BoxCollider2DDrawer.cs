using System.Collections.Generic;
using TimeLine.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.ECS.Services;
using TimeLine.LevelEditor.General;
using TimeLine.LevelEditor.InspectorTab.Components.BoxCollider;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using BoxCollider = Unity.Physics.BoxCollider;
using Material = Unity.Physics.Material;

namespace TimeLine.LevelEditor.InspectorTab.InspectorView.Drawers
{
    public class BoxCollider2DDrawer : IComponentDrawer
    {
        private KeyframeCreator _keyframeCreator;
        private CustomInspectorDrawer _customInspectorDrawer = null;
        private TrackObjectStorage _trackObjectStorage;
        private ColliderDrawer _colliderDrawer;

        public BoxCollider2DDrawer(ColliderDrawer colliderDrawer)
        {
            _colliderDrawer = colliderDrawer;
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
            return CheckIfComponentTypeInList.Check(component, typeof(BoxColliderData));
        }

        public void Draw(Entity target)
        {
            _customInspectorDrawer.CreateComponent(ComponentNames.BoxCollider, target, true);

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (entityManager.HasComponent<PhysicsCollider>(target) &&
                entityManager.HasComponent<BoxColliderData>(target))
            {
                PhysicsCollider startPhysicsCollider = entityManager.GetComponentData<PhysicsCollider>(target);
                BoxColliderData boxColliderData = entityManager.GetComponentData<BoxColliderData>(target);
                TrackObjectPacket trackObjectPacket = _trackObjectStorage.GetTrackObjectData(target);


                if (startPhysicsCollider.TryGetBox(out BoxGeometry geometry))
                {
                    float3 startCenter = geometry.Center;
                    _colliderDrawer.AddCollider(Components.BoxCollider.ColliderType.BoxCollider, target);
                    
                    _customInspectorDrawer.CreateFloatField(boxColliderData.boxSize.x, "Size/X", null,
                        (value) =>
                        {
                            boxColliderData.boxSize = new float3(value, boxColliderData.boxSize.y, 100);
                            entityManager.SetComponentData(target, boxColliderData);
                        }, trackObjectPacket, "BoxCollider.Size.X");

                    _customInspectorDrawer.CreateFloatField(boxColliderData.boxSize.y, "Size/Y", null,
                        (value) =>
                        {
                            boxColliderData.boxSize = new float3(boxColliderData.boxSize.y, value, 100);
                            entityManager.SetComponentData(target, boxColliderData);
                        }, trackObjectPacket, "BoxCollider.Size.Y");

                    _customInspectorDrawer.CreateFloatField(startCenter.x, "Offset/X", null,
                        (value) =>
                        {
                            boxColliderData.boxCenter = new float3(value, boxColliderData.boxCenter.y,
                                boxColliderData.boxCenter.z);
                            entityManager.SetComponentData(target, boxColliderData);
                        }, trackObjectPacket, "BoxCollider.Offset.X");
                    _customInspectorDrawer.CreateFloatField(startCenter.y, "Offset/Y", null,
                        (value) =>
                        {
                            boxColliderData.boxCenter = new float3(boxColliderData.boxCenter.x, value,
                                boxColliderData.boxCenter.z);
                            entityManager.SetComponentData(target, boxColliderData);
                        }, trackObjectPacket, "BoxCollider.Offset.Y");
                    _customInspectorDrawer.CreateBoolField(false, "IsTrigger",
                        (value) =>
                        {
                            boxColliderData.isTrigger = value;
                            entityManager.SetComponentData(target, boxColliderData);
                        });
                    _customInspectorDrawer.CreateBoolField(false, "IsDangerous",
                        (value) =>
                        {
                            boxColliderData.isDangerous = value;
                            entityManager.SetComponentData(target, boxColliderData);
                        });
                }
            }
        }
    }
}