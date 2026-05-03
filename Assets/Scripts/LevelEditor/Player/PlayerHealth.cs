using EventBus;
using TimeLine.EventBus.Events.Player;
using TimeLine.LevelEditor.Player;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private AudioSource hit; // Звук получения урона
        [SerializeField] private Slider healthSlider; // UI отображение здоровья
        [SerializeField] private PlayerDeath playerDeath; // Контроллер смерти
        [SerializeField] private PlayModeController playModeController; // Контроллер режима игры
        [Space]
        [SerializeField] private int maxHealth = 3; // Максимальное здоровье

        private int _currentHealth; // Текущее здоровье
        private GameEventBus _gameEventBus;

        // Внедрение зависимостей через Zenject
        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        // Метод восстановления здоровья
        private void RestoreHealth()
        {
            _currentHealth = maxHealth;
            healthSlider.value = _currentHealth;
        }

        private void Start()
        {
            // Инициализация слайдера
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
            _currentHealth = maxHealth;

            // Подписка на события
            _gameEventBus.SubscribeTo((ref PlayerHitEvent _) =>
            {
                TakeDamage();
            }, 5);
            _gameEventBus.SubscribeTo((ref TurnToPlayModeEvent _) =>
            {
                RestoreHealth();
            });
            _gameEventBus.SubscribeTo((ref RestartGameEvent data) => { RestoreHealth(); });
            // todo НЕТ ОТПИСКИ ОТ СОБЫТИЙ!
        }

        private void TakeDamage()
        {
            // Проверка на смерть игрока
            if (playerDeath.IsPlayerDeath) return;

            hit.Play(); // Проигрываем звук

            // Проверка режима игры
            if (playModeController.IsPlaying)
            {
                // Уменьшение здоровья
                _currentHealth--;
            }

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                _gameEventBus.Raise(new PlayerDeathEvent()); // Вызываем событие смерти
            }
            else
            {
                _gameEventBus.Raise(new PlayerInvulnerabilityStartedEvent());
            }
            healthSlider.value = _currentHealth; // Обновляем UI
        }
    }
}