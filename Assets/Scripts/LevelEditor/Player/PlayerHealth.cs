using EventBus;
using TimeLine.Player;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private AudioSource hit;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private PlayerDeath playerDeath;
        [SerializeField] private PlayModeController playModeController;
        [Space]
        [SerializeField] private int maxHealth = 3;

        private int _currentHealth;
        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void RestoreHealth()
        {
            _currentHealth = maxHealth;
            healthSlider.value = _currentHealth;
        }

        private void Start()
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
            _currentHealth = maxHealth;
            
            _gameEventBus.SubscribeTo((ref PlayerTakeDamageEvent _) =>
            {
                TakeDamage();
            });
            _gameEventBus.SubscribeTo((ref TurnToPlayModeEvent _) =>
            {
                RestoreHealth();
            });
            _gameEventBus.SubscribeTo((ref RestartGameEvent data) => { RestoreHealth(); });
        }

        private void TakeDamage()
        {
            if(playerDeath.IsPlayerDeath) return;
            
            hit.Play();
            
            if(!playModeController.IsPlaying) return;
            
            _currentHealth--;
            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                _gameEventBus.Raise(new PlayerDeathEvent());
            }
            healthSlider.value = _currentHealth;
        }
    }
}
