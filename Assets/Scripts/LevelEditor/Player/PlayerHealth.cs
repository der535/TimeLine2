using EventBus;
using TimeLine.Player;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class PlayerHealth : MonoBehaviour
    {
        // Хорошая группировка полей с Space для читаемости в Inspector
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
            _gameEventBus.SubscribeTo((ref PlayerTakeDamageEvent _) =>
            {
                TakeDamage();
            });
            _gameEventBus.SubscribeTo((ref TurnToPlayModeEvent _) =>
            {
                RestoreHealth();
            });
            _gameEventBus.SubscribeTo((ref RestartGameEvent data) => { RestoreHealth(); });
            // НЕТ ОТПИСКИ ОТ СОБЫТИЙ!
        }

        private void TakeDamage()
        {
            // Проверка на смерть игрока
            if (playerDeath.IsPlayerDeath) return;

            hit.Play(); // Проигрываем звук

            // Проверка режима игры
            if (!playModeController.IsPlaying) return;

            // Уменьшение здоровья
            _currentHealth--;
            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                _gameEventBus.Raise(new PlayerDeathEvent()); // Вызываем событие смерти
            }
            healthSlider.value = _currentHealth; // Обновляем UI
        }
    }
}