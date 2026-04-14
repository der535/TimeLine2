using UnityEngine;

namespace TimeLine.LevelEditor.Player
{
    public static class PlayerInvulnerable
    {
        public static bool IsInvulnerableAfterDamage { get; set; }
        public static bool IsInvulnerableAfterDash { get; set; }

        internal static bool IsInvulnerable()
        {
            return IsInvulnerableAfterDamage || IsInvulnerableAfterDash;
        }
    }
}