using System;
using TimeLine.Installers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class PositionTool : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [FormerlySerializedAs("_toolTransform")] [SerializeField] private RectTransform toolTransform;

        private MainObjects _mainObjects;
        private Canvas _canvas;

        private bool _isMovingY;
        private bool _isMovingX;
        private bool _isFreeMoving;

        private Vector2 _mouseOffset;
        private Vector2 _toolStartPosition;
        private Quaternion _inverseRotation;

        public Action<RectTransform> OnChangePosition;

        public bool isGlobal;

        [Inject]
        private void Construct(MainObjects mainObjects)
        {
            _mainObjects = mainObjects;
        }

        private void Awake()
        {
            _canvas = toolTransform.GetComponentInParent<Canvas>();
        }

        public void SetMoveY(bool isMovingY)
        {
            _isMovingY = isMovingY;
            if (isMovingY) SaveMouseOffset();
        }

        public void SetMoveX(bool isMovingX)
        {
            _isMovingX = isMovingX;
            if (isMovingX) SaveMouseOffset();
        }

        public void SetFreeMove(bool isFreeMoving)
        {
            _isFreeMoving = isFreeMoving;
            if (isFreeMoving) SaveMouseOffset();
        }

        private Vector2 GetMousePositionInParentSpace()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                toolTransform.parent as RectTransform,
                Mouse.current.position.ReadValue(),
                _canvas.worldCamera,
                out var localPosition);
            return localPosition;
        }

        private void SaveMouseOffset()
        {
            _mouseOffset = GetMousePositionInParentSpace();
            _toolStartPosition = toolTransform.anchoredPosition;
            
            // Сохраняем обратное вращение инструмента
            float rotationAngle = -toolTransform.localEulerAngles.z;
            _inverseRotation = Quaternion.Euler(0, 0, rotationAngle);
        }

        private void Update()
        {
            MoveY();
            MoveX();
            FreeMove();
        }

        private void MoveY()
        {
            if (!_isMovingY) return;

            Vector2 currentMousePos = GetMousePositionInParentSpace();
            Vector2 delta = currentMousePos - _mouseOffset;

            if (!isGlobal)
            {
                // Преобразуем дельту в локальное пространство инструмента
                Vector2 localDelta = _inverseRotation * delta;
                
                // Применяем только Y-компоненту
                Vector2 movement = new Vector2(0, localDelta.y);
                
                // Преобразуем движение обратно в глобальное пространство
                Vector2 globalMovement = Quaternion.Inverse(_inverseRotation) * movement;
                
                toolTransform.anchoredPosition = _toolStartPosition + globalMovement;
            }
            else
            {
                toolTransform.anchoredPosition = _toolStartPosition + new Vector2(0, delta.y);
            }

            OnChangePosition?.Invoke(toolTransform);
        }

        private void MoveX()
        {
            if (!_isMovingX) return;

            Vector2 currentMousePos = GetMousePositionInParentSpace();
            Vector2 delta = currentMousePos - _mouseOffset;

            if (!isGlobal)
            {
                // Преобразуем дельту в локальное пространство инструмента
                Vector2 localDelta = _inverseRotation * delta;
                
                // Применяем только X-компоненту
                Vector2 movement = new Vector2(localDelta.x, 0);
                
                // Преобразуем движение обратно в глобальное пространство
                Vector2 globalMovement = Quaternion.Inverse(_inverseRotation) * movement;
                
                toolTransform.anchoredPosition = _toolStartPosition + globalMovement;
            }
            else
            {
                toolTransform.anchoredPosition = _toolStartPosition + new Vector2(delta.x, 0);
            }

            OnChangePosition?.Invoke(toolTransform);
        }

        private void FreeMove()
        {
            if (!_isFreeMoving) return;

            Vector2 currentMousePos = GetMousePositionInParentSpace();
            Vector2 delta = currentMousePos - _mouseOffset;
            toolTransform.anchoredPosition = _toolStartPosition + delta;
            OnChangePosition?.Invoke(toolTransform);
        }
    }
}