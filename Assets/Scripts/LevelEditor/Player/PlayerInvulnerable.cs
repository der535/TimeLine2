using UnityEngine;

namespace TimeLine.LevelEditor.Player
{
    public class PlayerInvulnerable : MonoBehaviour
    {
        // Хорошо: свойство с публичным get и приватным set
        public bool IsInvulnerable { get; private set; }

        // Внутренний метод для изменения состояния
        internal void SetActive(bool value)
        {
            IsInvulnerable = value;
        }
    }
}