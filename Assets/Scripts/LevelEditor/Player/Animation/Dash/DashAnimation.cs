using System;
using TimeLine.LevelEditor.Player.Animation.Dash;
using UnityEngine;
using ShortcutExtensions = DG.Tweening.ShortcutExtensions;

namespace TimeLine
{
    public class DashAnimation : MonoBehaviour
    {
        [SerializeField] private DashCircleAnimation _dashCircleAnimation;
        [SerializeField] private DashParticleAnimation _dashParticleAnimation;
        [SerializeField] private PlayerMoveScale _playerMoveScale;
        [SerializeField] private DashPlayerColor _dashPlayerColor;

        public void Play()
        {
            _playerMoveScale.SetDashScale();
            _dashCircleAnimation.Play();
            _dashParticleAnimation.Play();
            _dashPlayerColor.Play();
        }

        public void Stop()
        {
            _playerMoveScale.SetNormalScale();
        }
    }
}