using System;
using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class SpriteRendererComponent : MonoBehaviour, ICopyableComponent
    {
        public SpriteParameter Sprite = new("Sprite", null, Color.magenta);
        [Space]
        public BoolParameter InvertX = new BoolParameter("InvertX", false, Color.grey);
        public BoolParameter InvertY = new BoolParameter("InvertY", false, Color.grey);
        [Space]
        public ColorParameter SpriteColor = new ColorParameter("SpriteColor", Color.white, Color.white);

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

        public void CopyTo(Component targetComponent)
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

        public Component Copy(GameObject targetGameObject)
        {
            var component = targetGameObject.GetComponent<SpriteRendererComponent>();
            CopyTo(component);
            return component;
        }
    }
}