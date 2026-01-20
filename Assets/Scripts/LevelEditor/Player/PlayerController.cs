using UnityEngine;
using Zenject;

namespace TimeLine
{
    // RequireComponent гарантирует наличие необходимых компонентов - хорошая практика
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        // Хорошо: сериализованные поля с Range для ограничения значений в Inspector
        [SerializeField, Range(0.1f, 20f)] private float _speed = 5.0f; // Стандартное значение для скорости

        // Опция для плавной остановки - хорошая идея для улучшения геймплея
        [SerializeField] private bool _useSmoothStop = false;

        // Коэффициент трения для плавной остановки
        [SerializeField, Range(0.0f, 1.0f)] private float _frictionCoefficient = 0.1f;

        // Приватные поля с нижним подчеркиванием - соответствует C# naming conventions
        private Rigidbody2D _rb;
        private ActionMap _actionMap; // Новый Input System
        private Vector2 _movementInput;

        // Внедрение зависимостей через Zenject
        [Inject]
        private void Constructor(ActionMap actionMap)
        {
            _actionMap = actionMap;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>(); // Получение компонента
        }

        // ОЖИДАЮТСЯ МЕТОДЫ:
        // - Start/OnEnable для подписки на события Input System
        // - OnDisable/OnDestroy для отписки
        // - FixedUpdate для физического движения
        // - Метод для обработки плавной остановки
        
         private void OnEnable()
            {
                // Включаем карту действий и подписываемся на события движения
                _actionMap.Player.Enable();
                _actionMap.Player.PlayerMove.performed += context => OnMove(context.ReadValue<Vector2>());
                _actionMap.Player.PlayerMove.canceled += context => OnMove(Vector2.zero);
            }
        
            private void OnDisable()
            {
                // Отписываемся от событий и отключаем карту действий
                _actionMap.Player.PlayerMove.performed -= context => OnMove(context.ReadValue<Vector2>());
                _actionMap.Player.PlayerMove.canceled -= context => OnMove(Vector2.zero);
                _actionMap.Player.Disable();
            }
        
            private void OnMove(Vector2 movement)
            {
                // Сохраняем входные данные для использования в FixedUpdate
                _movementInput = movement;
            }
        
            private void FixedUpdate()
            {
                // Используем сохраненные входные данные
                Vector2 inputDirection = _movementInput;
        
                // Проверяем, нажата ли какая-либо клавиша управления
                if (inputDirection.magnitude > 0.1f)
                {
                    // Если нажата, устанавливаем скорость
                    _rb.linearVelocity = inputDirection.normalized * _speed;
                }
                else
                {
                    // Если не нажата, останавливаем
                    if (_useSmoothStop)
                    {
                        // Плавная остановка: уменьшаем скорость
                        _rb.linearVelocity = _rb.linearVelocity * (1.0f - _frictionCoefficient);
                        // Дополнительно: можно добавить условие для полной остановки при очень маленькой скорости
                        if (_rb.linearVelocity.magnitude < 0.01f)
                        {
                            _rb.linearVelocity = Vector2.zero;
                        }
                    }
                    else
                    {
                        // Мгновенная остановка
                        _rb.linearVelocity = Vector2.zero;
                    }
                }
            }
    }

   
}