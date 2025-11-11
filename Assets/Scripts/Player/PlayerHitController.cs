using EventBus;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PlayerHitController : MonoBehaviour
    {
        [SerializeField] private float timeOfInvulnerability;
        [SerializeField] private PlayerHitAnimation playerHitAnimation;

        private GameEventBus _gameEventBus;
        private bool _isInvulnerable;
        
        public bool IsInvulnerable => _isInvulnerable;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref PlayerTakeDamageEvent playerTakeDamageEvent) =>
            {
                _isInvulnerable = true;
                playerHitAnimation.Play(timeOfInvulnerability, () => _isInvulnerable = false);
            });
        }
    }
}