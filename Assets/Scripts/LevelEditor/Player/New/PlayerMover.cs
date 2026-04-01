using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Player.New
{
    public class PlayerMover : MonoBehaviour
    {
        public float zposition;
        public float speed;
        private ActionMap _actionMap;
        private PlayerComponents _playerComponents;

        [Inject]
        private void Construct(ActionMap actionMap, PlayerComponents playerComponents)
        {
            _actionMap = actionMap;
            _playerComponents = playerComponents;
        }

        private void Update()
        {
            if (!_playerComponents.PlayerInitialized) return;
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var moveVector = _actionMap.Player.PlayerMove.ReadValue<Vector2>() * speed;

            LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(_playerComponents.Player);
            localTransform.Position.z = zposition;
            entityManager.SetComponentData(_playerComponents.Player, localTransform);

            // ПРОВЕРКА: Есть ли у сущности физика?
            if (entityManager.HasComponent<PhysicsVelocity>(_playerComponents.Player))
            {
                entityManager.SetComponentData(_playerComponents.Player, new PhysicsVelocity
                {
                    Linear = new float3(moveVector.x, moveVector.y, 0),
                    Angular = float3.zero
                });
            }
        }
    }
}