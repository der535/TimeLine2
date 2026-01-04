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
        public float currentRotation = 0f;
        public float rotationSpeed = 0.5f;
        public float startRotation = 0f;
        
        public Action<float> onRotate;
        
        private ActionMap _actionMap;

        public Action StartRotationAction;

        [Inject]
        private void Construct(ActionMap actionMap)
        {
            _actionMap = actionMap;
        }


        private void Start()
        {
            currentRotation = transform.eulerAngles.z;
            
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
            StartRotationAction.Invoke();
            isRotating = true;
            previousMousePosition = UnityEngine.Input.mousePosition;
            // Для внешнего использования сохраняем текущее видимое значение
            startRotation = tool.eulerAngles.z;
            accumulated_displacement = 0;
        }

        private float accumulated_displacement;

        private void ProcessRotation()
        {
            Vector2 currentMousePosition = UnityEngine.Input.mousePosition;
            Vector2 mouseDelta = currentMousePosition - previousMousePosition;
        
            // Вычисляем изменение угла
            float rotationDelta = -mouseDelta.x * rotationSpeed;
            
            // Обновляем накопленный угол (может быть любым, не только 0-360)
            currentRotation += rotationDelta;
            
            accumulated_displacement += rotationDelta;
            
            onRotate?.Invoke(accumulated_displacement);
        
            // Применяем поворот без ограничений
            tool.rotation = Quaternion.Euler(0, 0, currentRotation);
        
            previousMousePosition = currentMousePosition;
        }

        public void StopRotation()
        {
            isRotating = false;
        }
    }
}