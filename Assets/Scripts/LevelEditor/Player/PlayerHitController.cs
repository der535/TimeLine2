using EventBus;
using TimeLine.LevelEditor.Player;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PlayerHitController : IInitializable
    {
        private const float TimeOfInvulnerability = 2; // Время неуязвимости
        private PlayerHitAnimation _playerHitAnimation; // Анимация получения урона

        private GameEventBus _gameEventBus;

        // Внедрение зависимостей через Zenject
        [Inject]
        private void Constructor(GameEventBus gameEventBus, PlayerHitAnimation playerHitAnimation)
        {
            _gameEventBus = gameEventBus;
            _playerHitAnimation = playerHitAnimation;
        }

        public void Initialize()
        {
            // Подписка на событие получения урона
            _gameEventBus.SubscribeTo((ref PlayerTakeDamageEvent _) =>
            {
                PlayerInvulnerable.IsInvulnerableAfterDamage = true; // Включаем неуязвимость
                _playerHitAnimation.Play(TimeOfInvulnerability,
                    () => PlayerInvulnerable.IsInvulnerableAfterDamage = false); // Запускаем анимацию с колбэком
            });
            // todo НЕТ ОТПИСКИ ОТ СОБЫТИЙ!
        }
    }
}