using System;
using EventBus;
using Zenject;

namespace TimeLine.LevelEditor.Player.PlayerHealth
{
    public class PlayerHealthPresenter : IInitializable
    {
        private const int MaxHealth = 3; // Максимальное здоровье — теперь редактируется в инспекторе

        private PlayerDeath _playerDeath;
        private PlayModeController _playModeController; // Контроллер режима игры
        private PlayerHealthModel _model;
        private PlayerHealthView _view;
        private GameEventBus _gameEventBus;

        // Внедрение зависимостей через Zenject
        [Inject]
        private void Constructor(
            GameEventBus gameEventBus,
            PlayModeController playModeController, 
            PlayerDeath playerDeath,
            PlayerHealthView playerHealthView)
        {
            _gameEventBus = gameEventBus;
            _playModeController = playModeController;
            _playerDeath = playerDeath;
            _view = playerHealthView;
        }

        // Обработка получения урона
        private void HandleTakeDamage()
        {
            if (_playerDeath.IsPlayerDeath) return;
            _view.PlayHitSound();

            // if (!playModeController.IsPlaying) return;

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

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
