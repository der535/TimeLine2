using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.SpriteLoader;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class SpriteRendererComponent : BaseParameterComponent
    {
        public SpriteParameter Sprite = new("Sprite", null, Color.magenta);
        [Space]
        public IntParameter OrderInLayer = new("OrderInLayer", 0, Color.grey);
        [Space]
        public BoolParameter InvertX = new("InvertX", false, Color.grey);
        public BoolParameter InvertY = new("InvertY", false, Color.grey);
        [Space]
        public ColorParameter SpriteColor = new("SpriteColor", Color.white, Color.white);

        private SelectSpriteController _selectSpriteController;
        private SpriteRenderer _spriteRenderer;
        private PixelPerfectClick _pixelPerfectClick;
        private DiContainer _container;
        private CustomSpriteStorage _customSpriteStorage;
        
        [Inject]
        private void Construct(SelectSpriteController selectSpriteController, DiContainer container, CustomSpriteStorage customSpriteStorage)
        {
            _selectSpriteController = selectSpriteController;
            _container = container;
            _customSpriteStorage = customSpriteStorage;
            
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    
            // Если компонент не найден — добавляем его
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            _pixelPerfectClick = gameObject.AddComponent<PixelPerfectClick>();
            _container.Inject(_pixelPerfectClick);
            _pixelPerfectClick.Setup(_spriteRenderer);

            Sprite.OnValueChanged += () =>
            {
                _spriteRenderer.sprite = Sprite.Value;
            };

            OrderInLayer.OnValueChanged += () =>
            {
                _spriteRenderer.sortingOrder = OrderInLayer.Value;
            };
            
            InvertX.OnValueChanged += () =>
            {
                _spriteRenderer.flipX = InvertX.Value;
            };
    
            InvertY.OnValueChanged += () =>
            {
                _spriteRenderer.flipY = InvertY.Value;
            };
    
            SpriteColor.OnValueChanged += () =>
            {
                _spriteRenderer.color = SpriteColor.Value;
            };
        }

        private void OnDestroy()
        {
            _customSpriteStorage.CheckAndRemoveSpriteRenderer(Sprite);
            Destroy(_spriteRenderer);
            Destroy(_pixelPerfectClick);
        }

        protected override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return Sprite;
            yield return OrderInLayer;
            yield return InvertX;
            yield return InvertY;
            yield return SpriteColor;
        }
    }
}