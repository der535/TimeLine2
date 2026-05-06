using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace TimeLine
{
    public class RotateTool : MonoBehaviour
    {
        [SerializeField] private RectTransform tool;
        
        private bool isRotating;
        private Vector2 previousMousePosition;
        
        // Это значение должно быть "чистым" накопленным углом
        private float currentRotation = 0f; 
        public float rotationSpeed = 0.5f;
        
        public Action<float> onRotate;
        
        private ActionMap _actionMap;

        public Action StartRotationAction;
        public Action StopRotationAction;

        [Inject]
        private void Construct(ActionMap actionMap)
        {
            _actionMap = actionMap;
        }

        private void Start()
        {
            // УБРАНО: currentRotation = transform.eulerAngles.z; 
            // Мы не должны инициализировать это из eulerAngles, 
            // так как там углы всегда 0..360.
        }

        void Update()
        {
            if (isRotating)
            {
                ProcessRotation();
            
                if (_actionMap.Editor.MouseLeft.phase == InputActionPhase.Canceled)
                {
                    StopRotation();
                }
            }
        }

        public void StartRotation()
        {
            // 1. Сначала уведомляем контроллер, чтобы он зафиксировал StartRotation из RotationData
            StartRotationAction.Invoke(); 
            
            isRotating = true;
            previousMousePosition = UnityEngine.Input.mousePosition;
            
            // 2. Инициализируем локальный счетчик текущим визуальным углом инструмента.
            // Но важно: сам расчет дельты (accumulated_displacement) от этого не зависит.
            currentRotation = tool.eulerAngles.z;
            accumulated_displacement = 0;
        }

        private float accumulated_displacement;

        private void ProcessRotation()
        {
            Vector2 currentMousePosition = UnityEngine.Input.mousePosition;
            
            // Считаем дельту мыши
            Vector2 mouseDelta = currentMousePosition - previousMousePosition;
        
            // Вычисляем изменение угла на основе движения мыши
            float rotationDelta = -mouseDelta.x * rotationSpeed;
            
            // Накапливаем общее смещение с момента нажатия (deltaAngle для контроллера)
            accumulated_displacement += rotationDelta;
            
            // Вызываем событие. Контроллер прибавит это к своему StartRotation (-600)
            onRotate?.Invoke(accumulated_displacement);
        
            // Визуально поворачиваем гизмо
            currentRotation += rotationDelta;
            tool.rotation = Quaternion.Euler(0, 0, currentRotation);
        
            previousMousePosition = currentMousePosition;
        }

        public void StopRotation()
        {
            if (!isRotating) return;
            isRotating = false;
            StopRotationAction?.Invoke();
        }
    }
}