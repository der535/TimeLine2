using DG.Tweening;
using TimeLine.LevelEditor.Player;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class DashCircleAnimation : MonoBehaviour
    {
        [SerializeField] private GameObject dashCircle;
        [SerializeField] private Material dashCircleMaterial;
        [Space] 
        [SerializeField, Range(0, 0.5f)] private float _startRadius = 0.3f;
        [SerializeField] private float _RadiusDuraction = 0.3f;
        [Space] 
        [SerializeField] private float startThicknessAnimation = 0.1f;
        [SerializeField] private float _ThicknessDuraction = 0.3f;
        [Space] 
        [SerializeField] private float startColorAnimation = 0;
        [SerializeField] private float colorDuraction = 0;
        [SerializeField] private Color _startColor = Color.white;
        [SerializeField] private Color _endColor = Color.blue;
        
        private PlayerComponents _playerComponents;
        private Sequence _sequence;

        [Inject]
        private void Construct(PlayerComponents playerComponents)
        {
            _playerComponents = playerComponents;
        }

        private void Start()
        {
            SetupSequence();
        }

        private void SetupSequence()
        {
            DOTween.Init();
            
            dashCircleMaterial.SetFloat("_Radius", 0);
            dashCircleMaterial.SetFloat("_Thickness", 0.5f);
            dashCircleMaterial.SetColor("_MainColor", _startColor);
    
            _sequence = DOTween.Sequence();
    
            _sequence.Append(DOVirtual.Float(_startRadius, 0.5f, _RadiusDuraction, (value) =>
            {
                dashCircleMaterial.SetFloat("_Radius", value);
            }));
            _sequence.Insert(startColorAnimation, DOVirtual.Color(_startColor, _endColor, colorDuraction, (value) =>
            {
                dashCircleMaterial.SetColor("_MainColor", value);
            }));
            _sequence.Insert(startThicknessAnimation, DOVirtual.Float(0.5f, 0, _ThicknessDuraction, (value) =>
            {
                dashCircleMaterial.SetFloat("_Thickness", value);
            }));

            // ВАЖНО: Запрещаем самоудаление и ставим на паузу, чтобы не сработало само при старте
            _sequence.SetAutoKill(false).Pause();
        }
        
        public void Play()
        {
            dashCircle.transform.position = _playerComponents.GetPosition();
            _sequence.Restart();
        }
    }
}
