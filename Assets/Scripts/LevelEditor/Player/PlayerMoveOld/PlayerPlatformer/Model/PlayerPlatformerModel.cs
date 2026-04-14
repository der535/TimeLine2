using System;
using UnityEngine;

namespace TimeLine
{
    /// <summary>
    /// Данные игрока
    /// </summary>
    [Serializable]
    public class PlayerPlatformerModel : PlayerDataModel
    {
        public float GravityForce;
        public float Weight;
        public float JumpForce;
        public GravitationDirection GravitationDirection;

        public PlayerPlatformerModel(float baseSpeed, float gravity, float weight, float jumpForce,
            GravitationDirection gravitation) : base(baseSpeed)
        {
            GravityForce = gravity;
            Weight = weight;
            JumpForce = jumpForce;
            GravitationDirection = gravitation;
        }
    }

    public enum GravitationDirection
    {
        Down,
        Up,
        Left,
        Right
    }
}