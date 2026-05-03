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

            _actionMap.Player.PlayerMove.performed += HandleMove;
            _actionMap.Player.Space.performed += HandleSpace;
            _actionMap.Player.Space.canceled += HandleCancelSpace;
            InputActive = true;
            OnInputChange?.Invoke(true);
        }

        public void OnDisable()
        {
            _actionMap.Player.PlayerMove.performed -= HandleMove;
            _actionMap.Player.Space.performed -= HandleSpace;
            _actionMap.Player.Space.canceled -= HandleCancelSpace;

            _actionMap.Player.Disable();
            InputActive = false;
            OnInputChange?.Invoke(false);
        }
        
        private void HandleMove(InputAction.CallbackContext context)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();

            OnMovePerformed?.Invoke(moveInput);
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