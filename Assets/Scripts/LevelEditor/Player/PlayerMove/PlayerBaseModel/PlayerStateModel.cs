using UnityEngine;

namespace TimeLine
{
    public class PlayerStateModel
    {
        /// <summary>
        /// Направление игрока
        /// </summary>
        public Vector2 MovementInput { get; set; } = Vector2.zero;

        /// <summary>
        /// Двигается ли игрок
        /// </summary>
        public bool IsMoving { get; set; } = false;
        
        /// <summary>
        /// Скорость применяемая к игроку в данный момент
        /// </summary>
        public float CurrentSpeed { get; set; }
        
        /// <summary>
        /// Скорость применяемая к игроку в данный момент
        /// </summary>
        public Vector2 LastMoveInput { get; set; } = Vector2.zero;
    }
}