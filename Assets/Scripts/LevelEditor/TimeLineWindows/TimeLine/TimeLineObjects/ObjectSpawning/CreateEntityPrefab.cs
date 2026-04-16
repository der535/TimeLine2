using TimeLine.LevelEditor.ECS;
using TimeLine.LevelEditor.ECS.Components;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning
{
    public class CreateEntityPrefab
    {
        public Entity EntityPrefab { get; }
        
        public CreateEntityPrefab()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            // 1. Создаем архетип
            EntityArchetype archetype = entityManager.CreateArchetype(
                typeof(LocalTransform),
                typeof(PositionData),
                typeof(RotationData),
                typeof(EntityActiveTag),
                typeof(PostTransformMatrix), // Добавим, раз он был в твоем примере
                typeof(Prefab),
                typeof(ObjectPositionOffsetData),
                typeof(NameComponent)
            );

            // 2. Создаем сущность
            Entity prefabEntity = entityManager.CreateEntity(archetype);
            entityManager.SetName(prefabEntity, "MyPureCodePrefab");

            // 3. ЗАПИСЫВАЕМ ДАННЫЕ СРАЗУ
            // Теперь все копии будут рождаться с этими значениями
            entityManager.SetComponentData(prefabEntity, LocalTransform.FromPosition(0, 0, 0));
            

            entityManager.SetComponentData(prefabEntity, new PostTransformMatrix { 
                Value = float4x4.identity 
            });

            EntityPrefab = prefabEntity;
        }
    }
}