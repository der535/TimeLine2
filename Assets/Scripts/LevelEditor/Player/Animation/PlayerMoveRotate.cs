using System;
using TimeLine.LevelEditor.Player;
using TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PlayerMoveRotate : MonoBehaviour
    {
        [SerializeField] private WindowsFocus _windowsFocus;
        
        private PlayModeController _playModeController;
        private PlayerComponents _playerComponents;
        private ActionMap _actionMap;
        private PlayerInputView _playerInputView;
        
        [Inject]
        private void Construct(
            ActionMap actionMap,
            PlayerComponents playerComponents,
            PlayerInputView playerInputView,
            PlayModeController playModeController)
        {
            _actionMap = actionMap;
            _playerComponents = playerComponents;
            _playerInputView = playerInputView;
            _playModeController = playModeController;
        }
        public void Update()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if(!entityManager.Exists( _playerComponents.Player))return;

            var newMoveInput = _playerInputView.GetMoveDirection();

            if (newMoveInput.sqrMagnitude > 0.01f)
            {
                float3 eulerRadians = math.Euler(Quaternion.identity);
                float3 eulerDegrees = math.degrees(eulerRadians);
                float angle = Mathf.Atan2(newMoveInput.y, newMoveInput.x) * Mathf.Rad2Deg-90;
                
                LocalTransform transform = entityManager.GetComponentData<LocalTransform>(_playerComponents.Player);
                eulerDegrees.z = angle;
                transform.Rotation = quaternion.Euler(math.radians(eulerDegrees));
                entityManager.SetComponentData(_playerComponents.Player, transform);
            }
        }
    }
}