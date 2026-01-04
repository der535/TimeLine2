using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace TimeLine
{
    public class ShakeCamera : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform cameraTransform;

        private Vector3 _originalPos;
        private Tween _shakeTween;
        private Vector3 _currentOffset;

        private void Start()
        {
            // Запоминаем позицию в начале (с учетом Z)
            if (cameraTransform == null) cameraTransform = transform;
            _originalPos = cameraTransform.position;
            _originalPos.z = -10;
        }

        [Button]
        public void Shake(Vector2 shakeStrength, float duration, int vibrato, float randomness)
        {
            // Останавливаем предыдущую тряску, если она была
            _shakeTween?.Kill();
            _currentOffset = Vector3.zero;

            // Создаем "виртуальную" тряску вектора смещения
            // Мы трясем Vector3, но фактически используем только X и Y
            _shakeTween = DOTween.Shake(() => _currentOffset, x => _currentOffset = x, duration, 
                    new Vector3(shakeStrength.x, shakeStrength.y, 0), vibrato, randomness)
                .OnUpdate(() =>
                {
                    // Применяем смещение к оригинальной позиции, игнорируя Z из тряски
                    cameraTransform.position = new Vector3(
                        _originalPos.x + _currentOffset.x,
                        _originalPos.y + _currentOffset.y,
                        _originalPos.z // Z всегда остается константой
                    );
                })
                .OnComplete(() =>
                {
                    // Возвращаем в точный ноль по окончании
                    cameraTransform.position = _originalPos;
                });
        }
    }
}
