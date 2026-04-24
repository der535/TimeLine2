using EventBus;
using TimeLine.Player;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PlayerHealthPresenter : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 3; // Максимальное здоровье — теперь редактируется в инспекторе
        [SerializeField] private PlayerDeath playerDeath; // Контроллер смерти
        [SerializeField] private PlayModeController playModeController; // Контроллер режима игры

        private PlayerHealthModel _model;
        private PlayerHealthView _view;
        private GameEventBus _gameEventBus;

        // Внедрение зависимостей через Zenject
        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            // Создаём экземпляр Model с заданным maxHealth
            _model = new PlayerHealthModel(maxHealth);
            _view = GetComponent<PlayerHealthView>();

            // Инициализируем компоненты
            _model.Initialize();
            _view.Initialize(_model.MaxHealth);

            // Подписка на события
            _gameEventBus.SubscribeTo((ref PlayerTakeDamageEvent _) =>
            {
                HandleTakeDamage();
            });
            _gameEventBus.SubscribeTo((ref TurnToPlayModeEvent _) =>
            {
                HandleRestoreHealth();
            });
            _gameEventBus.SubscribeTo((ref RestartGameEvent data) => { HandleRestoreHealth(); });
        }

        // Обработка получения урона
        private void HandleTakeDamage()
        {
            if (playerDeath.IsPlayerDeath) return;
            _view.PlayHitSound();

            if (!playModeController.IsPlaying) return;

            bool isDead = _model.TakeDamage();
            _view.UpdateHealthUI(_model.CurrentHealth);

            if (isDead)
            {
                _gameEventBus.Raise(new PlayerDeathEvent());
            }
        }

        // Обработка восстановления здоровья
        private void HandleRestoreHealth()
        {
            _model.RestoreHealth();
            _view.UpdateHealthUI(_model.CurrentHealth);
        }
    }
}
