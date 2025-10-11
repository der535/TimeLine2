using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic;
using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class SpriteRendererComponent : BaseParameterComponent
    {
        public SpriteParameter Sprite = new("Sprite", null, Color.magenta);
        [Space]
        public BoolParameter InvertX = new("InvertX", false, Color.grey);
        public BoolParameter InvertY = new("InvertY", false, Color.grey);
        [Space]
        public ColorParameter SpriteColor = new("SpriteColor", Color.white, Color.white);

        private SelectSpriteController _selectSpriteController;
        private SpriteRenderer _spriteRenderer;
        
        [Inject]
        private void Construct(SelectSpriteController selectSpriteController)
        {
            _selectSpriteController = selectSpriteController;
        }
        
        private void Awake()
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            
            Sprite.OnValueChanged += (() =>
            {
                _spriteRenderer.sprite = Sprite.Value;
            });
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

        protected override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return Sprite;
            yield return InvertX;
            yield return InvertY;
            yield return SpriteColor;
        }

        public override void CopyTo(Component targetComponent)
        {
            if (targetComponent is SpriteRendererComponent other)
            {
                other.Sprite.Value = Sprite.Value;
            }
            else
            {
                throw new ArgumentException("Target component must be of type NameComponent");
            }
        }

        public override Component Copy(GameObject targetGameObject)
        {
            var component = targetGameObject.GetComponent<SpriteRendererComponent>();
            CopyTo(component);
            return component;
        }
    }
}