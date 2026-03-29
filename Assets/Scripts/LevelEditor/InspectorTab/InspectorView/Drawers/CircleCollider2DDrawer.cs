using System.Collections.Generic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.General;
using TimeLine.LevelEditor.InspectorTab.Components.BoxCollider;
using TimeLine.LevelEditor.InspectorTab.InspectorView.Drawers;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using ColliderType = TimeLine.LevelEditor.InspectorTab.Components.BoxCollider.ColliderType;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.InspectorView.Drawers
{
    public class CircleCollider2DDrawer : IComponentDrawer
    {
        private KeyframeCreator _keyframeCreator;
        private CustomInspectorDrawer _customInspectorDrawer = null;
        private TrackObjectStorage _trackObjectStorage = null;
        private ColliderDrawer _colliderDrawer;
        private ToolsController _toolsController;

        public CircleCollider2DDrawer(ColliderDrawer colliderDrawer)
        {
            _colliderDrawer = colliderDrawer;
        }

        public void Setup(CustomInspectorDrawer customInspectorDrawer, TrackObjectStorage trackObjectStorage,
            KeyframeCreator keyframeCreator, ToolsController toolsController, TimeLineRecorder timeLineRecorder)
        {
            _customInspectorDrawer = customInspectorDrawer;
            _keyframeCreator = keyframeCreator;
            _trackObjectStorage = trackObjectStorage;
            _toolsController = toolsController;
        }


        public bool GetComponent(List<ComponentType> component)
        {
            return CheckIfComponentTypeInList.Check(component, typeof(CircleColliderData));
        }

        public void Draw(Entity target)
        {
            
            _customInspectorDrawer.CreateComponent(ComponentNames.CircleCollider, target, true);
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            CircleColliderData circleColliderData = entityManager.GetComponentData<CircleColliderData>(target);


            if (entityManager.HasComponent<PhysicsCollider>(target))
            {
                PhysicsCollider startPhysicsCollider = entityManager.GetComponentData<PhysicsCollider>(target);

                if (startPhysicsCollider.TryGetSphere(out SphereGeometry geometry))
                {
                    float radius = geometry.Radius;
                    float3 startCenter = geometry.Center;
                    TrackObjectPacket trackObjectPacket = _trackObjectStorage.GetTrackObjectData(target);


                    
                    var posX = new FloatParameter("posx", 0, Color.white);
                    _toolsController.OnValueChanged += () =>
                    {
                        LocalTransform transform = entityManager.GetComponentData<LocalTransform>(target);
                        posX.Value = transform.Position.x;
                    };
                    
                    
                    
                    _colliderDrawer.AddCollider(ColliderType.CircleCollider, target);

                    _customInspectorDrawer.CreateFloatField(radius, "Radius", null,
                        (value) =>
                        {
                            circleColliderData.radius = value;
                            entityManager.SetComponentData(target, circleColliderData);
                        },trackObjectPacket, "CircleCollider.Radius");

                    _customInspectorDrawer.CreateBoolField(circleColliderData.isTrigger, "IsTrigger", 
                        (value) =>
                        {
                            circleColliderData.isTrigger = value;
                            entityManager.SetComponentData(target, circleColliderData);
                        });

                    _customInspectorDrawer.CreateFloatField(startCenter.x, "Offset/X", null,
                        (value) =>
                        {
                            circleColliderData.center.x = value;
                            entityManager.SetComponentData(target, circleColliderData);
                        }, trackObjectPacket, "CircleCollider.Offset.X");
                    _customInspectorDrawer.CreateFloatField(startCenter.y, "Offset/Y", null,
                        (value) =>
                        {
                            circleColliderData.center.y = value;
                            entityManager.SetComponentData(target, circleColliderData);
                        }, trackObjectPacket, "CircleCollider.Offset.Y");
                    _customInspectorDrawer.CreateBoolField(false, "IsDangerous",
                        (value) =>
                        {
                            circleColliderData.isDangerous = value;
                            entityManager.SetComponentData(target, circleColliderData);
                        });
                }
            }
        }
    }
}