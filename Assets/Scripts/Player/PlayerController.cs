using UnityEngine;

namespace TimeLine
{
    [RequireComponent(typeof(Rigidbody2D))] // Убедитесь, что на объекте есть Rigidbody2D
    public class PlayerController : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 20f)]
        private float _speed = 5.0f;

        // Новое поле: использовать ли плавную остановку
        [SerializeField]
        private bool _useSmoothStop = false;

        // Новое поле: коэффициент трения/затухания для плавной остановки
        [SerializeField, Range(0.0f, 1.0f)]
        private float _frictionCoefficient = 0.1f;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            // Явно указываем UnityEngine.Input
            float moveX = UnityEngine.Input.GetAxisRaw("Horizontal");
            float moveY = UnityEngine.Input.GetAxisRaw("Vertical");

            Vector2 inputDirection = new Vector2(moveX, moveY);

            // Проверяем, нажата ли какая-либо клавиша управления
            if (inputDirection.magnitude > 0.1f) // Используем небольшой порог, чтобы избежать дребезга
            {
                // Если нажата, устанавливаем скорость
                _rb.velocity = inputDirection.normalized * _speed;
            }
            else
            {
                // Если не нажата, останавливаем
                if (_useSmoothStop)
                {
                    // Плавная остановка: уменьшаем скорость
                    _rb.velocity = _rb.velocity * (1.0f - _frictionCoefficient);
                    // Дополнительно: можно добавить условие для полной остановки при очень маленькой скорости
                    if (_rb.velocity.magnitude < 0.01f)
                    {
                        _rb.velocity = Vector2.zero;
                    }
                }
                else
                {
                    // Мгновенная остановка
                    _rb.velocity = Vector2.zero;
                }
            }
        }
    }
}