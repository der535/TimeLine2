using EventBus;
using TimeLine.LevelEditor.Player;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PlayerHitController : MonoBehaviour
    {
        [SerializeField] private float timeOfInvulnerability;
        [SerializeField] private PlayerHitAnimation playerHitAnimation;
        [SerializeField] private PlayerInvulnerable playerInvulnerable;

        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref PlayerTakeDamageEvent playerTakeDamageEvent) =>
            {
                playerInvulnerable.SetActive(true);
                playerHitAnimation.Play(timeOfInvulnerability, () => playerInvulnerable.SetActive(false));
            });
        }
    }
}