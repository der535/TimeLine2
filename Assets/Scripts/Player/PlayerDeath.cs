using EventBus;
using UnityEngine;
using Zenject;

namespace TimeLine.Player
{
    public class PlayerDeath : MonoBehaviour
    {
        [SerializeField] private ParticleSystem deathParticles;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private GameEventBus _gameEventBus;
        private ActionMap _actionMap;
        
        internal bool IsPlayerDeath;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, ActionMap actionMap)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
        }
        private void Start()
        {
            _gameEventBus.SubscribeTo((ref PlayerDeathEvent data) => Death());
        }

        private void Death()
        {
            IsPlayerDeath = true;
            spriteRenderer.enabled = false;
            _actionMap.Player.Disable();
            deathParticles.Play();
        }
    }
}