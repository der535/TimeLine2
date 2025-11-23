using UnityEngine;

namespace TimeLine.LevelEditor.Player
{
    public class PlayerInvulnerable : MonoBehaviour
    {
        public bool IsInvulnerable { get; private set; }

        internal void SetActive(bool value)
        {
            IsInvulnerable = value;
        }
    }
}