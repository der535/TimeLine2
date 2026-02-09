using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove
{
    public class PlayerFreeMoveController : ITickable
    {
        private PlayerFreeMoveAnimationView _playerFreeMoveAnimationView;
        private PlayerFreeMoveRigidbodyView _playerFreeMoveRigidbodyView;
        private PlayerInputView _playerInputView;

        private PlayerFreeMoveModel _data;
        private PlayerStateModel _state = new PlayerStateModel();

        [Inject]
        private void Constructor(PlayerFreeMoveRigidbodyView playerFreeMoveRigidbodyView,
            PlayerFreeMoveAnimationView playerFreeMoveAnimationView, PlayerInputView playerInputView)
        {
            _playerFreeMoveRigidbodyView = playerFreeMoveRigidbodyView;
            _playerFreeMoveAnimationView = playerFreeMoveAnimationView;
            _playerInputView = playerInputView;
        }

        public void Enable(PlayerFreeMoveModel model)
        {
            _data = model;

            _state.CurrentSpeed = _data.BaseSpeed;

            _playerInputView.OnSpacePerformed += OnDashPerformed;
            _playerInputView.OnMovePerformed += OnMovePerformed;
        }

        public void Disable()
        {
            _data = null;
            if (_playerInputView == null) return;
            _playerInputView.OnSpacePerformed -= OnDashPerformed;
            _playerInputView.OnMovePerformed -= OnMovePerformed;
        }

        public float CalculateAngle(Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        }

        private void OnMove(Vector2 movement)
        {
            // Сохраняем входные данные для использования в FixedUpdate
            _state.MovementInput = movement;
            if (_state.MovementInput != Vector2.zero)
                _playerFreeMoveAnimationView.SetVelocity(CalculateAngle(_state.MovementInput));
        }

        private void OnDashPerformed()
        {
            if (_state.IsMoving)
                _playerFreeMoveAnimationView.Dash(_data.DashDuration);

            _state.CurrentSpeed = _data.DashSpeed;
            DOVirtual.DelayedCall(_data.DashDuration, () => { _state.CurrentSpeed = _data.BaseSpeed; });
        }

        private void OnMovePerformed(Vector2 movement)
        {
            if (movement == _state.LastMoveInput) return;
            _state.LastMoveInput = movement;
            _state.IsMoving = true;
            _playerFreeMoveAnimationView.Move();
            OnMove(movement);
        }


        public void Tick()
        {
            if (_data == null) return;
            // Используем сохраненные входные данные
            Vector2 inputDirection = _state.MovementInput;

            // Проверяем, нажата ли какая-либо клавиша управления
            if (inputDirection.magnitude > 0.1f)
            {
                // Если нажата, устанавливаем скорость
                _playerFreeMoveRigidbodyView.SetVelocity(inputDirection.normalized * _state.CurrentSpeed);
            }
            else if (_state.IsMoving)
            {
                _state.IsMoving = false;
                _playerFreeMoveAnimationView.NotMove();
                _playerFreeMoveRigidbodyView.SetVelocity(Vector2.zero);
            }
        }
    }
}