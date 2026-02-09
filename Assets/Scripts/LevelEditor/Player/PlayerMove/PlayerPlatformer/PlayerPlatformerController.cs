using System;
using TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PlayerPlatformerController : ITickable
    {
        private PlayerFreeMoveRigidbodyView _playerRigidbodyView;
        private PlayerInputView _playerInputView;

        private GroundCheckController _groundCheckController;

        private PlayerPlatformerModel _data;
        private PlayerPlatformerStateModel _state = new PlayerPlatformerStateModel();

        [Inject]
        private void Constructor(PlayerFreeMoveRigidbodyView playerFreeMoveRigidbodyView,
            GroundCheckController groundCheckController, PlayerInputView playerInputView)
        {
            _playerRigidbodyView = playerFreeMoveRigidbodyView;
            _groundCheckController = groundCheckController;
            _playerInputView = playerInputView;
        }

        public void Enable(PlayerPlatformerModel data)
        {
            _data = data;

            _playerRigidbodyView.SetWeight(data.Weight);
            _state.CurrentSpeed = _data.BaseSpeed;

            _playerInputView.OnSpacePerformed += OnJumpPerformed;
            _playerInputView.OnSpaceCanceled += OnJumpCanceled;
            _playerInputView.OnMovePerformed += OnMove;
        }

        public void Disable()
        {
            _data = null;
            if (_playerInputView == null) return;
            _playerInputView.OnSpacePerformed -= OnJumpPerformed;
            _playerInputView.OnSpaceCanceled -= OnJumpCanceled;
            _playerInputView.OnMovePerformed -= OnMove;
        }

        private void OnMove(Vector2 movement)
        {
            // Сохраняем входные данные для использования в FixedUpdate
            _state.MovementInput = movement;
        }


        public void Tick()
        {
            if (_data == null) return;
            // Используем сохраненные входные данные
            Vector2 inputDirection = _state.MovementInput;

            if (_data.GravitationDirection == GravitationDirection.Down ||
                _data.GravitationDirection == GravitationDirection.Up)
            {
                // Проверяем, нажата ли какая-либо клавиша управления
                if (inputDirection.magnitude > 0.1f)
                {
                    // Если нажата, устанавливаем скорость
                    _playerRigidbodyView.SetVelocity(new Vector2(inputDirection.x * _state.CurrentSpeed,
                        _playerRigidbodyView.GetVelocity().y));
                }
                else
                {
                    _playerRigidbodyView.SetVelocity(new Vector2(0, _playerRigidbodyView.GetVelocity().y));
                }
            }
            else
            {
                // Проверяем, нажата ли какая-либо клавиша управления
                if (inputDirection.magnitude > 0.1f)
                {
                    // Если нажата, устанавливаем скорость
                    _playerRigidbodyView.SetVelocity(new Vector2(_playerRigidbodyView.GetVelocity().x,
                        inputDirection.y * _state.CurrentSpeed));
                }
                else
                {
                    _playerRigidbodyView.SetVelocity(new Vector2(_playerRigidbodyView.GetVelocity().x, 0));
                }
            }


            Vector2 force = Vector2.zero;

            switch (_data.GravitationDirection)
            {
                case GravitationDirection.Down:
                    force = new Vector2(0, -_data.GravityForce);
                    break;
                case GravitationDirection.Up:
                    force = new Vector2(0, _data.GravityForce);
                    break;
                case GravitationDirection.Left:
                    force = new Vector2(-_data.GravityForce, 0);
                    break;
                case GravitationDirection.Right:
                    force = new Vector2(_data.GravityForce, 0);
                    break;
            }

            _playerRigidbodyView.AddForce(force, ForceMode2D.Force);
        }

        private void OnJumpPerformed()
        {
            if (_groundCheckController.IsGrounded(_data.GravitationDirection))
            {
                _state.JumpStopped = false;

                Vector2 force = Vector2.zero;

                switch (_data.GravitationDirection)
                {
                    case GravitationDirection.Down:
                        force = new Vector2(0, _data.JumpForce);
                        break;
                    case GravitationDirection.Up:
                        force = new Vector2(0, -_data.JumpForce);
                        break;
                    case GravitationDirection.Left:
                        force = new Vector2(_data.JumpForce, 0);
                        break;
                    case GravitationDirection.Right:
                        force = new Vector2(-_data.JumpForce, 0);
                        break;
                }

                _playerRigidbodyView.AddForce(force, ForceMode2D.Impulse);
            }
        }

        private void OnJumpCanceled()
        {
            if (_data.GravitationDirection == GravitationDirection.Down)
            {
                if (_playerRigidbodyView.GetVelocity().y <= 0 || _state.JumpStopped)
                {
                    _state.JumpStopped = true;
                    return;
                }

                _playerRigidbodyView.SetVelocity(new Vector2(_playerRigidbodyView.GetVelocity().x, 0));
            }

            if (_data.GravitationDirection == GravitationDirection.Up)
            {
                if (_playerRigidbodyView.GetVelocity().y >= 0 || _state.JumpStopped)
                {
                    _state.JumpStopped = true;
                    return;
                }

                _playerRigidbodyView.SetVelocity(new Vector2(_playerRigidbodyView.GetVelocity().x, 0));
            }
            
            
            if (_data.GravitationDirection == GravitationDirection.Left)
            {
                if (_playerRigidbodyView.GetVelocity().x <= 0 || _state.JumpStopped)
                {
                    _state.JumpStopped = true;
                    return;
                }

                _playerRigidbodyView.SetVelocity(new Vector2(0, _playerRigidbodyView.GetVelocity().y));
            }

            
            if (_data.GravitationDirection == GravitationDirection.Right)
            {
                if (_playerRigidbodyView.GetVelocity().x >= 0 || _state.JumpStopped)
                {
                    _state.JumpStopped = true;
                    return;
                }

                _playerRigidbodyView.SetVelocity(new Vector2(0, _playerRigidbodyView.GetVelocity().y));
            }
        }
    }
}