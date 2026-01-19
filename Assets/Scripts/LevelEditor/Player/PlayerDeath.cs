using DG.Tweening;
using EventBus;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.Player
{
    public class PlayerDeath : MonoBehaviour
    {
        // Сериализованные поля - хорошо для настройки через Inspector
        [SerializeField] private ParticleSystem deathParticles; // Эффект смерти
        [SerializeField] private SpriteRenderer spriteRenderer; // Визуал игрока
        [SerializeField] private TimeLineRestartAnimation restartAnimation; // Анимация рестарта

        private GameEventBus _gameEventBus;
        private ActionMap _actionMap; // Input System
        private Main _main; // Возможно, главный контроллер игры

        internal bool IsPlayerDeath; // Внутреннее состояние - хорошо

        // Внедрение зависимостей - правильный подход
        [Inject]
        private void Constructor(GameEventBus gameEventBus, ActionMap actionMap, Main main)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
            _main = main;
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
            spriteRenderer.enabled = false; // Скрываем спрайт
            _actionMap.Player.Disable(); // Отключаем управление
            deathParticles.Play(); // Запускаем эффект смерти

            restartAnimation.Play(); // Запускаем анимацию рестарта
        }
    }
}