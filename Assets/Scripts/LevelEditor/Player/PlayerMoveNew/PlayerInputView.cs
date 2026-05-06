using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove
{
    public class PlayerInputView : MonoBehaviour
    {
        private ActionMap _actionMap;
        
        public event Action<Vector2> OnMovePerformed;
        public event Action OnSpacePerformed;
        public event Action OnSpaceCanceled;
        public event Action<bool> OnInputChange;
        public bool InputActive { get; private set; }

        [Inject]
        private void Construct(ActionMap actionMap)
        {
            _actionMap = actionMap;
        }

        public void OnEnable()
        {
            _actionMap.Player.Enable();
            _actionMap.Player.PlayerMoveWASD.performed += HandleMove;
            _actionMap.Player.PlayerMoveArrows.performed += HandleMove;
            _actionMap.Player.GamePad.performed += HandleMove;
            _actionMap.Player.Space.performed += HandleSpace;
            _actionMap.Player.Space.canceled += HandleCancelSpace;
            InputActive = true;
            OnInputChange?.Invoke(true);
        }

        public void OnDisable()
        {
            _actionMap.Player.PlayerMoveWASD.performed -= HandleMove;
            _actionMap.Player.PlayerMoveArrows.performed -= HandleMove;
            _actionMap.Player.GamePad.performed -= HandleMove;
            _actionMap.Player.Space.performed -= HandleSpace;
            _actionMap.Player.Space.canceled -= HandleCancelSpace;

            _actionMap.Player.Disable();
            InputActive = false;
            OnInputChange?.Invoke(false);
        }
        
        private void HandleMove(InputAction.CallbackContext context)
        {
            OnMovePerformed?.Invoke(GetMoveDirection());
        }

        public Vector2 GetMoveDirection()
        {
            Vector2 raw1 = _actionMap.Player.PlayerMoveWASD.ReadValue<Vector2>();
            Vector2 raw2 = _actionMap.Player.PlayerMoveArrows.ReadValue<Vector2>();
            
            // Преобразуем в дискретные направления ( -1, 0, +1 )
            Vector2 discrete1 = SnapTo8Directions(raw1);
            Vector2 discrete2 = SnapTo8Directions(raw2);

            // Складываем
            Vector2 combined = discrete1 + discrete2;

            // Ограничиваем каждую ось диапазоном [-1, 1]
            combined.x = Mathf.Clamp(combined.x, -1f, 1f);
            combined.y = Mathf.Clamp(combined.y, -1f, 1f);

            // Снова квантуем сумму (на случай, если появились нецелые)
            combined = SnapTo8Directions(combined);

            // Если нужна одинаковая скорость по диагонали и по осям — нормализуем
            if (combined.sqrMagnitude > 0.01f)
            {
                combined.Normalize();
            }

            Vector2 raw3 = _actionMap.Player.GamePad.ReadValue<Vector2>();
            if (raw3.sqrMagnitude > 0.01f) combined = raw3;
            return combined; 
        }

        private Vector2 SnapTo8Directions(Vector2 input)
        {
            float threshold = 0.2f; // отсекаем шум для аналоговых стиков
            float x = Mathf.Abs(input.x) > threshold ? Mathf.Sign(input.x) : 0f;
            float y = Mathf.Abs(input.y) > threshold ? Mathf.Sign(input.y) : 0f;
            return new Vector2(x, y);
        }
        
        private void HandleSpace(InputAction.CallbackContext context) 
        {
            OnSpacePerformed?.Invoke();
        }
        
        private void HandleCancelSpace(InputAction.CallbackContext context) 
        {
            OnSpaceCanceled?.Invoke();
        }
    }
}