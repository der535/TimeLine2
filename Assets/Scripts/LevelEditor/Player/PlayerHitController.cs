using EventBus;
using TimeLine.LevelEditor.Player;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PlayerHitController : MonoBehaviour
    {
        [SerializeField] private float timeOfInvulnerability; // Время неуязвимости
        [SerializeField] private PlayerHitAnimation playerHitAnimation; // Анимация получения урона
        [SerializeField] private PlayerInvulnerable playerInvulnerable; // Состояние неуязвимости

        private GameEventBus _gameEventBus;

        // Внедрение зависимостей через Zenject
        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            // Подписка на событие получения урона
            _gameEventBus.SubscribeTo((ref PlayerTakeDamageEvent playerTakeDamageEvent) =>
            {
                playerInvulnerable.SetActive(true); // Включаем неуязвимость
                playerHitAnimation.Play(timeOfInvulnerability, () => playerInvulnerable.SetActive(false)); // Запускаем анимацию с колбэком
            });
            // НЕТ ОТПИСКИ ОТ СОБЫТИЙ!
        }
    }
}