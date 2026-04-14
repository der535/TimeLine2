using System;
using DG.Tweening;
using TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Player.PlayerMoveNew.PlayerFreeMove
{
    public class PlayerMover : MonoBehaviour
    {
        public float zposition;
        public float speed;
        public float dashSpeed;
        public float dashDuraction = 1;
        private float currentSpeed;
        private bool _isDashing;
        
        private PlayerComponents _playerComponents;
        private PlayerInputView _playerInputView;
        [SerializeField] DashAnimation _dashAnimation;

        private Action<Vector2> _onMovePerformed;
        private Vector2 _savedVelocity;

        [Inject]
        private void Construct(PlayerComponents playerComponents, PlayerInputView playerInputView)
        {
            _playerComponents = playerComponents;
            _playerInputView = playerInputView;
        }

        private void Start()
        {
            currentSpeed = speed;
            
            _playerInputView.OnSpacePerformed += () =>
            {
                currentSpeed = dashSpeed;
                _dashAnimation.Play();
                _isDashing = true;
                PlayerInvulnerable.IsInvulnerableAfterDash = true;
                DOVirtual.DelayedCall(dashDuraction, () =>
                {
                    currentSpeed = speed;
                    _isDashing = false;
                    PlayerInvulnerable.IsInvulnerableAfterDash = false;
                    _dashAnimation.Stop();
                    _onMovePerformed.Invoke(_savedVelocity);
                });
                _onMovePerformed.Invoke(_savedVelocity);
            };
            
            _onMovePerformed += vector2 =>
            {
                if (!_playerComponents.PlayerInitialized) return;
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                
                var moveVector = vector2 * currentSpeed;
                _savedVelocity = vector2;

                LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(_playerComponents.Player);
                localTransform.Position.z = zposition;
                entityManager.SetComponentData(_playerComponents.Player, localTransform);
                
                if (entityManager.HasComponent<PhysicsVelocity>(_playerComponents.Player))
                {
                    entityManager.SetComponentData(_playerComponents.Player, new PhysicsVelocity
                    {
                        Linear = new float3(moveVector.x, moveVector.y, 0),
                        Angular = float3.zero
                    });
                }
            };

            _playerInputView.OnMovePerformed += _onMovePerformed;
        }
    }
}