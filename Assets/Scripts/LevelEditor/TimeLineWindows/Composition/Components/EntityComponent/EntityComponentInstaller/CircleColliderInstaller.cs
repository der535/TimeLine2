using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using UnityEngine;
using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentInstaller
{
    public class CircleColliderInstaller : IComponentInstaller
    {
        public ComponentNames GetComponentName()
        {
            return ComponentNames.CircleCollider;
        }

        public void Install(Entity entity)
        {
            Install(entity, new CircleColliderData()
            {
                radius = 0.5f,
                center = Vector3.zero,
                isTrigger = false,
            });
        }

        public void Install(Entity entity, CircleColliderData circleColliderData)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // 1. Описываем геометрию круга
            var geometry = new SphereGeometry
            {
                Center = float3.zero, // Центр круга относительно позиции сущности
                Radius = 0.5f // Радиус круга
            };

            var filter = CollisionFilter.Default;

            // 2. Создаем BlobAssetReference для сферы (в 2D это будет круг)
            BlobAssetReference<Collider> collider = SphereCollider.Create(geometry, filter);

            // 3. Добавляем компонент на сущность
            // Если компонент уже есть, лучше использовать SetComponentData
            if (entityManager.HasComponent<PhysicsCollider>(entity))
            {
                entityManager.SetComponentData(entity, new PhysicsCollider { Value = collider });
            }
            else
            {
                entityManager.AddComponentData(entity, new PhysicsCollider { Value = collider });
            }

            entityManager.AddComponentData(entity, circleColliderData);
            entityManager.AddSharedComponentManaged(entity, new PhysicsWorldIndex
            {
                Value = 0
            });
            entityManager.AddComponent<ColliderTag>(entity);

        }

        public void Remove(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (entityManager.HasComponent<PhysicsCollider>(entity))
            {
                entityManager.RemoveComponent<PhysicsCollider>(entity);
                entityManager.RemoveComponent<CircleColliderData>(entity);
                entityManager.RemoveComponent<PhysicsWorldIndex>(entity);
                entityManager.RemoveComponent<ColliderTag>(entity);
            }
        }
    }
}