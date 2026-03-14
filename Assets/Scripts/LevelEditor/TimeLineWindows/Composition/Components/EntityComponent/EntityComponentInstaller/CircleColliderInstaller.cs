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

            // Описываем "коробку", которая плоская по оси Z
            var geometry = new BoxGeometry
            {
                Center = float3.zero,
                Size = new float3(1f, 1f, 0.05f), // Увеличим толщину до 0.05 для стабильности
                Orientation = quaternion.identity,
                BevelRadius = 0.0f // Для плоских объектов скругление часто не нужно
            };

            var filter = CollisionFilter.Default;

            // Создаем BlobAsset
            BlobAssetReference<Collider> collider = BoxCollider.Create(geometry, filter);
            

            // Добавляем на сущность
            entityManager.AddComponentData(entity, new PhysicsCollider { Value = collider });
        }

        public void Remove(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            if (entityManager.HasComponent<PhysicsCollider>(entity))
            {
                entityManager.RemoveComponent<PhysicsCollider>(entity);
            }
        }
    }
}