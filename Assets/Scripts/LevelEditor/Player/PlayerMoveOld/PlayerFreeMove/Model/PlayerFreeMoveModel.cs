using System;

namespace TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove
{
    [Serializable]
    public class PlayerFreeMoveModel : PlayerDataModel
    {
        /// <summary>
        /// Максимальная скорость при dash
        /// </summary>
        public float DashSpeed;
        /// <summary>
        /// Продолжительно работы деша с утиханием до базовой
        /// </summary>
        public float DashDuration;
        
        public PlayerFreeMoveModel(float baseSpeed, float dashSpeed, float dashDuration) : base(baseSpeed)
        {
            DashSpeed = dashSpeed;
            DashDuration = dashDuration;
        }
    }
}