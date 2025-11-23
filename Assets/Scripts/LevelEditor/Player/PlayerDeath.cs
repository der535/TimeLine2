using DG.Tweening;
using EventBus;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.Player
{
    public class PlayerDeath : MonoBehaviour
    {
        [SerializeField] private ParticleSystem deathParticles;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TimeLineRestartAnimation restartAnimation;
        
        private GameEventBus _gameEventBus;
        private ActionMap _actionMap;
        private Main _main;
        
        internal bool IsPlayerDeath;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, ActionMap actionMap, Main main)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
            _main = main;
        }
        private void Start()
        {
            _gameEventBus.SubscribeTo((ref PlayerDeathEvent data) => Death());
            _gameEventBus.SubscribeTo((ref RestartGameEvent data) => IsPlayerDeath = false);
        }

        private void Death()
        {
            IsPlayerDeath = true;
            spriteRenderer.enabled = false;
            _actionMap.Player.Disable();
            deathParticles.Play();

            restartAnimation.Play();
        }
    }
}