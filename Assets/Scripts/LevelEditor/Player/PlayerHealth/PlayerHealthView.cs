using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class PlayerHealthView : MonoBehaviour
    {
        [SerializeField] private AudioSource hit; // Звук получения урона
        [SerializeField] private Slider healthSlider; // UI отображение здоровья

        // Инициализация UI
        public void Initialize(int maxHealth)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

        // Обновление UI здоровья
        public void UpdateHealthUI(int currentHealth)
        {
            healthSlider.value = currentHealth;
        }

        // Воспроизведение звука урона
        public void PlayHitSound()
        {
            hit.Play();
        }
    }
}