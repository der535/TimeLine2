using System;
using TimeLine.LevelEditor.Player;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PlayerMoveRotate : MonoBehaviour
    {
        private PlayerComponents _playerComponents;
        private ActionMap _actionMap;
        [Inject]
        private void Construct(ActionMap actionMap, PlayerComponents playerComponents)
        {
            _actionMap = actionMap;
            _playerComponents = playerComponents;
        }
        public void Update()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if(!entityManager.Exists( _playerComponents.Player))return;
            
            Vector2 moveInput = _actionMap.Player.PlayerMove.ReadValue<Vector2>();

            if (moveInput.sqrMagnitude > 0.01f) // Проверка, что стик не в покое
            {
                // Atan2 принимает (y, x). Результат в радианах.
                
                float3 eulerRadians = math.Euler(Quaternion.identity);
                float3 eulerDegrees = math.degrees(eulerRadians);
                float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg-90;
                
                LocalTransform transform = entityManager.GetComponentData<LocalTransform>(_playerComponents.Player);
                eulerDegrees.z = angle;
                transform.Rotation = quaternion.Euler(math.radians(eulerDegrees));
                entityManager.SetComponentData(_playerComponents.Player, transform);
            }
        }
    }
}