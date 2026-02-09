using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove
{
    public class PlayerInputView : MonoBehaviour
    {
        private ActionMap _actionMap;
        
        private bool _isMoving = false;

        public event Action<Vector2> OnMovePerformed;
        public event Action OnSpacePerformed;
        public event Action OnSpaceCanceled;

        [Inject]
        private void Construct(ActionMap actionMap)
        {
            _actionMap = actionMap;
        }

        private void OnEnable()
        {
            _actionMap.Player.Enable();

            _actionMap.Player.PlayerMove.performed += HandleMove;
            _actionMap.Player.Space.performed += HandleSpace;
            _actionMap.Player.Space.canceled += HandleCancelSpace;
        }

        private void OnDisable()
        {
            _actionMap.Player.PlayerMove.performed -= HandleMove;
            _actionMap.Player.Space.performed -= HandleSpace;
            _actionMap.Player.Space.canceled -= HandleCancelSpace;

            _actionMap.Player.Disable();
        }
        
        private void HandleMove(InputAction.CallbackContext context)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();

            OnMovePerformed?.Invoke(moveInput);
        }
        
        private void HandleSpace(InputAction.CallbackContext context) 
        {
            Debug.Log("<color=yellow>[Space]</color> Performed");
            OnSpacePerformed?.Invoke();
        }
        
        private void HandleCancelSpace(InputAction.CallbackContext context) 
        {
            Debug.Log("<color=white>[Space]</color> Canceled");
            OnSpaceCanceled?.Invoke();
        }
    }
}