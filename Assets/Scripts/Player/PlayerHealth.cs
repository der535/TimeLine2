using EventBus;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private AudioSource hit;
        [SerializeField] private Slider healthSlider;
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
        }

        private void TakeDamage()
        {
            hit.Play();
            print(_currentHealth);
            _currentHealth--;
            print(_currentHealth);
            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                _gameEventBus.Raise(new PlayerDeathEvent());
            }
            print(_currentHealth);
            healthSlider.value = _currentHealth;
        }
    }
}
