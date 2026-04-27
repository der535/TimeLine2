namespace TimeLine
{
    public class PlayerHealthModel
    {
        private int _maxHealth; // Максимальное здоровье (передаётся извне)
        private int _currentHealth; // Текущее здоровье

        public int MaxHealth => _maxHealth;
        public int CurrentHealth
        {
            get => _currentHealth;
            set => _currentHealth = value;
        }

        // Конструктор с настройкой максимального здоровья
        public PlayerHealthModel(int maxHealth)
        {
            _maxHealth = maxHealth;
        }

        // Инициализация здоровья
        public void Initialize()
        {
            _currentHealth = _maxHealth;
        }

        // Уменьшение здоровья на 1
        public bool TakeDamage()
        {
            _currentHealth--;
            return _currentHealth <= 0;
        }

        // Восстановление полного здоровья
        public void RestoreHealth()
        {
            _currentHealth = _maxHealth;
        }
    }
}