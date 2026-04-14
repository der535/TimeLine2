using System;
using DG.Tweening;
using NaughtyAttributes;
using TimeLine.LevelEditor.Player;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;
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
        Sequence sequence;

        [Inject]
        private void Construct(PlayerComponents playerComponents)
        {
            _playerComponents = playerComponents;
        }

        private void Start()
        {
            SetupSequence();
        }
        [Button]
        private void SetupSequence()
        {
            DOTween.Init();
    
            // Подготовка материала
            dashCircleMaterial.SetFloat("_Radius", 0);
            dashCircleMaterial.SetFloat("_Thickness", 0.5f);
            dashCircleMaterial.SetColor("_MainColor", _startColor);
    
            sequence = DOTween.Sequence();
    
            sequence.Append(DOVirtual.Float(_startRadius, 0.5f, _RadiusDuraction, (value) =>
            {
                dashCircleMaterial.SetFloat("_Radius", value);
            }));
            sequence.Insert(startColorAnimation, DOVirtual.Color(_startColor, _endColor, colorDuraction, (value) =>
            {
                dashCircleMaterial.SetColor("_MainColor", value);
            }));
            sequence.Insert(startThicknessAnimation, DOVirtual.Float(0.5f, 0, _ThicknessDuraction, (value) =>
            {
                dashCircleMaterial.SetFloat("_Thickness", value);
            }));

            // ВАЖНО: Запрещаем самоудаление и ставим на паузу, чтобы не сработало само при старте
            sequence.SetAutoKill(false).Pause();
        }

        [Button]
        public void Play()
        {
            dashCircle.transform.position = _playerComponents.GetPosition();
    
            // Используем Restart, чтобы сбросить таймлайн в 0 и проиграть заново
            sequence.Restart();
        }
    }
}
