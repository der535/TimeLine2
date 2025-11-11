using EventBus;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PlayerCollision:MonoBehaviour
    {
        [SerializeField] private PlayerHitController playerHitController;

        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(TagsStorage.IsDamageable) && !playerHitController.IsInvulnerable)
            {
                _gameEventBus.Raise(new PlayerTakeDamageEvent());
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(TagsStorage.IsDamageable) && !playerHitController.IsInvulnerable)
            {
                _gameEventBus.Raise(new PlayerTakeDamageEvent());
            }
        }
    }
}