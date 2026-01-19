using EventBus;
using TimeLine.LevelEditor.Player;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PlayerCollision : MonoBehaviour
    {
        // Сериализованные поля - хорошая практика для настройки через Inspector
        [SerializeField] private PlayerHitController playerHitController; // Пока не используется в коде
        [SerializeField] private PlayerInvulnerable playerInvulnerable; // Важно для проверки неуязвимости

        private GameEventBus _gameEventBus;

        // Внедрение зависимостей через Zenject - отличный подход для слабой связанности
        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        // Обработка физических столкновений (Collider + Rigidbody)
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(TagsStorage.IsDamageable) && !playerInvulnerable.IsInvulnerable)
            {
                // Использование EventBus - хорошая архитектурная практика
                _gameEventBus.Raise(new PlayerTakeDamageEvent());
            }
        }

        // Обработка триггерных столкновений (Is Trigger)
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Дублирование логики - можно оптимизировать
            if (collision.gameObject.CompareTag(TagsStorage.IsDamageable) && !playerInvulnerable.IsInvulnerable)
            {
                _gameEventBus.Raise(new PlayerTakeDamageEvent());
            }
        }
    }
}