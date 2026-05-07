using EventBus;
using TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove;
using TimeLine.LevelEditor.Player.PlayerMoveNew.PlayerFreeMove.View;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Player
{
    public class PlayerDeath : MonoBehaviour
    {
        // Сериализованные поля - хорошо для настройки через Inspector
        [SerializeField] private ParticleSystem deathParticles; // Эффект смерти

        private GameEventBus _gameEventBus;
        private PlayerComponents _playerComponents;
        private TimeLineRestartAnimation _restartAnimation; // Анимация рестарта
        private PlayerInputView _playerInputView;

        internal bool IsPlayerDeath; // Внутреннее состояние - хорошо

        // Внедрение зависимостей - правильный подход
        [Inject]
        private void Constructor(
            GameEventBus gameEventBus,
            PlayerComponents playerComponents,
            TimeLineRestartAnimation restartAnimation,
            PlayerTrailContoller playerTrailContoller,
            PlayerInputView playerInputView)
        {
            _gameEventBus = gameEventBus;
            _playerComponents = playerComponents;
            _restartAnimation = restartAnimation;
            _playerInputView = playerInputView;
        }

        private void Start()
        {
            // Подписка на события
            _gameEventBus.SubscribeTo((ref PlayerDeathEvent _) => Death());
            _gameEventBus.SubscribeTo((ref RestartGameEvent _) => IsPlayerDeath = false);
            // НЕТ ОТПИСКИ - потенциальная проблема!
        }

        private void Death()
        {
            _playerInputView.OnDisable();
            IsPlayerDeath = true;
            _playerComponents.ChangeActive(false);
            deathParticles.transform.position = _playerComponents.GetPosition();
            deathParticles.Play(); // Запускаем эффект смерти
            _restartAnimation.Play(); // Запускаем анимацию рестарта
        }
    }
}