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
        [SerializeField] DashAnimation _dashAnimation;
        [SerializeField] private WindowsFocus _windowsFocus;

        public float zposition;
        public float speed;
        public float dashSpeed;
        public float dashDuraction = 1;
        public float cooldownDuraction = 0.1f;
        private float currentSpeed;
        private bool _isDashing;
        private bool _canDash;

        private PlayModeController _playModeController;
        private PlayerComponents _playerComponents;
        private PlayerInputView _playerInputView;
        public Action<Vector2> _onMovePerformed;
        private Vector2 _savedVelocity;
        private Vector2 _moveVector;

        [Inject]
        private void Construct(PlayerComponents playerComponents, PlayerInputView playerInputView, PlayModeController playModeController)
        {
            _playModeController = playModeController;
            _playerComponents = playerComponents;
            _playerInputView = playerInputView;
        }

        private void Start()
        {
            currentSpeed = speed;
            _canDash = true;

            _playerInputView.OnSpacePerformed += () =>
            {
                if (_canDash) _canDash = false;
                else return;

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
                    DOVirtual.DelayedCall(cooldownDuraction, () => { _canDash = true; });
                });
                _onMovePerformed.Invoke(_savedVelocity);
            };

            _onMovePerformed += vector2 =>
            {
                if (_playModeController.IsPlaying == false && _windowsFocus.IsFocused == false) return;
                
                if (!_playerComponents.PlayerInitialized) return;
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                _moveVector = vector2 * currentSpeed;
                _savedVelocity = vector2;

                LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(_playerComponents.Player);
                localTransform.Position.z = zposition;
                entityManager.SetComponentData(_playerComponents.Player, localTransform);
            };

            _playerInputView.OnInputChange += b => { _onMovePerformed?.Invoke(Vector2.zero); };

            _playerInputView.OnMovePerformed += _onMovePerformed;
        }

        private void Update()
        {
            var moveVector = !_playerInputView.InputActive ? new float3(0) : new float3(_moveVector.x, _moveVector.y, 0);

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (entityManager.HasComponent<PhysicsVelocity>(_playerComponents.Player))
            {
                entityManager.SetComponentData(_playerComponents.Player, new PhysicsVelocity
                {
                    Linear = moveVector,
                    Angular = float3.zero
                });
            }
        }
    }
}