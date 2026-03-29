using System.Collections.Generic;
using TimeLine.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.General;
using TimeLine.LevelEditor.InspectorTab.Components.BoxCollider;
using TimeLine.LevelEditor.InspectorTab.InspectorView.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using ColliderType = TimeLine.LevelEditor.InspectorTab.Components.BoxCollider.ColliderType;

namespace TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers
{
    public class PolygonCollider2DDrawer : IComponentDrawer
    {
        private KeyframeCreator _keyframeCreator;
        private CustomInspectorDrawer _customInspectorDrawer = null;
        private ColliderDrawer _colliderDrawer = null;

        public PolygonCollider2DDrawer(ColliderDrawer colliderDrawer)
        {
            _colliderDrawer = colliderDrawer;
        }

        public void Setup(CustomInspectorDrawer customInspectorDrawer, TrackObjectStorage trackObjectStorage,
            KeyframeCreator keyframeCreator, ToolsController toolsController, TimeLineRecorder timeLineRecorder)
        {
            _customInspectorDrawer = customInspectorDrawer;
            _keyframeCreator = keyframeCreator;
        }

        public bool GetComponent(List<ComponentType> component)
        {
            // Debug.Log($"Polygon Collider2DDrawer { CheckIfComponentTypeInList.Check(component, typeof(PolygonColliderTag))}");
            return CheckIfComponentTypeInList.Check(component, typeof(PolygonColliderData));
        }

        public void Draw(Entity target)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            PolygonColliderData polygonColliderData = entityManager.GetComponentData<PolygonColliderData>(target);
            _customInspectorDrawer.CreateComponent(ComponentNames.PolygonCollider, target, true);
            _colliderDrawer.AddCollider(ColliderType.PolygonCollider, target);
            _customInspectorDrawer.CreateEditColliderButton();

            _customInspectorDrawer.CreateBoolField(polygonColliderData.IsTrigger, "Is trigger", (data) =>
            {
                polygonColliderData.IsTrigger = data;
                entityManager.SetComponentData(target, polygonColliderData);
            });
            _customInspectorDrawer.CreateBoolField(false, "IsDangerous",
                (value) =>
                {
                    polygonColliderData.IsDangerous = value;
                    entityManager.SetComponentData(target, polygonColliderData);
                });
        }
    }
}