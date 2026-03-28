using DG.Tweening;
using EventBus;
using TimeLine.LevelEditor.Player;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.Player
{
    public class PlayerDeath : MonoBehaviour
    {
        // Сериализованные поля - хорошо для настройки через Inspector
        [SerializeField] private ParticleSystem deathParticles; // Эффект смерти

        private GameEventBus _gameEventBus;
        private ActionMap _actionMap; // Input System
        private PlayerComponents _playerComponents;
        private TimeLineRestartAnimation _restartAnimation; // Анимация рестарта

        internal bool IsPlayerDeath; // Внутреннее состояние - хорошо

        // Внедрение зависимостей - правильный подход
        [Inject]
        private void Constructor(GameEventBus gameEventBus, ActionMap actionMap, PlayerComponents playerComponents, TimeLineRestartAnimation restartAnimation)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
            _playerComponents = playerComponents;
            _restartAnimation = restartAnimation;
        }

        private void Start()
        {
            // Подписка на события
            _gameEventBus.SubscribeTo((ref PlayerDeathEvent data) => Death());
            _gameEventBus.SubscribeTo((ref RestartGameEvent data) => IsPlayerDeath = false);
            // НЕТ ОТПИСКИ - потенциальная проблема!
        }

        private void Death()
        {
            IsPlayerDeath = true;
            _playerComponents.SetActive(false);
            _actionMap.Player.Disable(); // Отключаем управление
            deathParticles.Play(); // Запускаем эффект смерти
            _restartAnimation.Play(); // Запускаем анимацию рестарта
        }
    }
}