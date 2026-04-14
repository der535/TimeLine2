using UnityEngine;

namespace TimeLine
{
    /// <summary>
    /// Хранит скорость игрока
    /// </summary>
    public abstract class PlayerDataModel
    {
        /// <summary>
        /// Базовая скорость передвижения
        /// </summary>
        public float BaseSpeed;

        public PlayerDataModel(float baseSpeed)
        {
            BaseSpeed = baseSpeed;
        }
    }
}