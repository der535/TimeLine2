using EventBus;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Zenject;
using Material = Unity.Physics.Material;


namespace TimeLine.LevelEditor.Player
{
    public class PlayerComponents : IInitializable
    {
        public Entity Player;
        public UnityEngine.Material PlayerMaterial;
        public bool PlayerInitialized;
        
        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        public void Initialize()
        {
            _gameEventBus.SubscribeTo((ref LevelLoadedEvent levelLoadedEvent) =>
            {
                PlayerInitialized = true;
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                EntityQuery query = entityManager.CreateEntityQuery(typeof(PlayerTag));
                Player = query.GetSingletonEntity();

                RenderMeshArray rma = entityManager.GetSharedComponentManaged<RenderMeshArray>(Player);
                if (entityManager.HasComponent<MaterialMeshInfo>(Player))
                {
                    var meshInfo = entityManager.GetComponentData<MaterialMeshInfo>(Player);

                    // Получаем текущий материал  
                    PlayerMaterial = rma.GetMaterial(meshInfo);
                }
            });
        }

        // public void ChangeCollider()
        // {
        //     var geometry = new BoxGeometry
        //     {
        //         Center = new float3(0, 0, 0),
        //         Size = new float3(1, 1, 100),
        //         Orientation = quaternion.identity,
        //         BevelRadius = 0.02f
        //     };
        //
        //     var material = new Material
        //     {
        //         CollisionResponse = CollisionResponsePolicy.CollideRaiseCollisionEvents,
        //         Friction = 0.5f,
        //         Restitution = 0f
        //     };
        //
        //     // CollisionResponsePolicy.RaiseTriggerEvents
        //     // CollisionResponsePolicy.CollideRaiseCollisionEvents,
        //
        //     // Создаем новый. 
        //     // УДАЛЯЕМ ручной .Dispose() старого colliderRef.ValueRO.Value!
        //     // Unity сама очистит память, когда вы перезапишете ссылку ниже.
        //     var newCollider = Unity.Physics.BoxCollider.Create(geometry, CollisionFilter.Default, material);
        //
        //     EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //     PhysicsCollider physicsCollider = entityManager.GetComponentData<PhysicsCollider>(Player);
        //     physicsCollider.Value = newCollider;
        // }

        public void ChangeActive(bool active)
        {
            // Получаем систему, которая отвечает за выполнение команд в конце кадра
            var ecbSystem = World.DefaultGameObjectInjectionWorld
                .GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
    
            // Создаем буфер (НЕ вызываем Playback сами!)
            var ecb = ecbSystem.CreateCommandBuffer();
            
            if (active)
            {
                ecb.AddComponent<PlayerTag>(Player);
                SetAlfa(1);
            }
            else
            {
                ecb.RemoveComponent<PlayerTag>(Player);
                SetAlfa(0);
            }
        }

        public float3 GetPosition()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(Player);
            return localTransform.Position;
        }

        public void SetAlfa(float alfa)
        {
            Color color = PlayerMaterial.color;
            color.a = alfa;
            PlayerMaterial.color = color;
        }

        public Color GetColor()
        {
            return PlayerMaterial.color;
        }
    }
}