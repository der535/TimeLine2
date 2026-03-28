using UnityEngine;

namespace TimeLine.LevelEditor.Player
{
    public static class PlayerInvulnerable
    {
        // Хорошо: свойство с публичным get и приватным set
        public static bool IsInvulnerable { get; private set; }

        // Внутренний метод для изменения состояния
        internal static void SetActive(bool value)
        {
            IsInvulnerable = value;
        }
    }
}