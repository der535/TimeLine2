using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Collider = Unity.Physics.Collider;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentInstaller
{
    public class BoxColliderInstaller : IComponentInstaller
    {
        public ComponentNames GetComponentName()
        {
            return ComponentNames.BoxCollider;
        }

        public void Install(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            entityManager.AddComponentData(entity, new BoxColliderData()
            {
                boxSize = new float3(1,1,100)
            });

            // Сделаем коробку потолще по Z для тестов (например, 1.0f)
            // Если это 2D, это никак не помешает, но сделает физику стабильнее
            var geometry = new BoxGeometry
            {
                Center = float3.zero,
                Size = new float3(1), // 1x1x1 куб
                Orientation = quaternion.identity,
                BevelRadius = 0 // Небольшой скос помогает избежать "застревания" на стыках
            };

            // "Всевидящий" фильтр: принадлежит всем (All), сталкивается со всеми (All)
            var filter = new CollisionFilter
            {
                BelongsTo = ~0u,    // 0xffffffff
                CollidesWith = ~0u, // 0xffffffff
                GroupIndex = 0
            };

            BlobAssetReference<Collider> collider = BoxCollider.Create(geometry, filter);
    
            // Если на сущности уже есть коллайдер, его нужно сначала удалить (из памяти), 
            // но для простоты пока просто добавляем:
            entityManager.AddComponentData(entity, new PhysicsCollider { Value = collider });
            entityManager.AddSharedComponentManaged(entity, new PhysicsWorldIndex
            {
                Value = 0
            });

        }

        public void Install(Entity entity, BoxColliderData boxColliderData)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            // Сделаем коробку потолще по Z для тестов (например, 1.0f)
            // Если это 2D, это никак не помешает, но сделает физику стабильнее
            var geometry = new BoxGeometry
            {
                Center = boxColliderData.boxCenter,
                Size = boxColliderData.boxSize, // 1x1x1 куб
                Orientation = quaternion.identity,
                BevelRadius = 0 // Небольшой скос помогает избежать "застревания" на стыках
            };

            // "Всевидящий" фильтр: принадлежит всем (All), сталкивается со всеми (All)
            var filter = new CollisionFilter
            {
                BelongsTo = ~0u,    // 0xffffffff
                CollidesWith = ~0u, // 0xffffffff
                GroupIndex = 0
            };
            
                        
            // 1. Создаем материал и помечаем его как триггер
            var material = new Material
            {
                // Это превращает коллайдер в триггер
                CollisionResponse = boxColliderData.isTrigger ? CollisionResponsePolicy.RaiseTriggerEvents : CollisionResponsePolicy.CollideRaiseCollisionEvents,
                Friction = 0.5f,
                Restitution = 0f,
                CustomTags = 0
            };


            BlobAssetReference<Collider> collider = BoxCollider.Create(geometry, filter, material);
    
            // Если на сущности уже есть коллайдер, его нужно сначала удалить (из памяти), 
            // но для простоты пока просто добавляем:
            entityManager.AddComponentData(entity, new PhysicsCollider { Value = collider });
            entityManager.AddSharedComponentManaged(entity, new PhysicsWorldIndex
            {
                Value = 0
            });

        }

        public void Remove(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            if (entityManager.HasComponent<PhysicsCollider>(entity))
            {
                entityManager.RemoveComponent<PhysicsCollider>(entity);
                entityManager.RemoveComponent<BoxColliderData>(entity);
                entityManager.RemoveComponent<PhysicsWorldIndex>(entity);
            }
        }
    }
}