using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace TimeLine.LevelEditor.Player.Animation.Dash
{
    public class DashPlayerColor : MonoBehaviour
    {
        [SerializeField] private Material _playerMaterial;
        [SerializeField] private TrailRenderer _trailRenderer;
        [Space] 
        [SerializeField] private Color startColor;
        [SerializeField] private Color endColor;
        [SerializeField] private float animationDuration = 0.2f;

        public void Play()
        {
            DOVirtual.Color(startColor, endColor, animationDuration, value =>
            {
                _playerMaterial.SetColor("_BaseColor", value);
                _trailRenderer.colorGradient.colorKeys[0].color = value;
                _trailRenderer.colorGradient.colorKeys[1].color = value;
            });
        }
    }
}