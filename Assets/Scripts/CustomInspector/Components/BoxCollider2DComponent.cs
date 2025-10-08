using System;
using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class BoxCollider2DComponent : MonoBehaviour, ICopyableComponent
    {
        public FloatParameter OffsetX = new FloatParameter("OffsetX", 0, Color.yellow);
        public FloatParameter OffsetY = new FloatParameter("OffsetY", 0, Color.yellow);

        public FloatParameter SizeX = new FloatParameter("SizeX", 1, Color.red);
        public FloatParameter SizeY = new FloatParameter("SizeY", 1, Color.red);

        private SelectSpriteController _selectSpriteController;
        private SpriteRenderer _spriteRenderer;

        private BoxCollider2DOutline _boxCollider2DOutline;

        [Inject]
        private void Construct(SelectSpriteController selectSpriteController, DiContainer container, CollidersPrefab collidersPrefab)
        {
            _selectSpriteController = selectSpriteController;
            _boxCollider2DOutline = container.InstantiatePrefab(collidersPrefab.BoxCollider2DPrefab, transform).GetComponent<BoxCollider2DOutline>();
        }

        private void Awake()
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

            print(_boxCollider2DOutline);
            print(_boxCollider2DOutline.BoxCollider);

            OffsetX.Value = _boxCollider2DOutline.BoxCollider.size.x;
            OffsetY.Value = _boxCollider2DOutline.BoxCollider.size.y;
            SizeX.Value = _boxCollider2DOutline.BoxCollider.size.x;
            SizeY.Value = _boxCollider2DOutline.BoxCollider.size.y;

            OffsetX.OnValueChanged += () =>
            {
                _boxCollider2DOutline.BoxCollider.offset =
                    new Vector2(OffsetX.Value, _boxCollider2DOutline.BoxCollider.offset.y);
                _boxCollider2DOutline.UpdateOutline();
            };
            OffsetY.OnValueChanged += () =>
            {
                _boxCollider2DOutline.BoxCollider.offset =
                    new Vector2(_boxCollider2DOutline.BoxCollider.offset.x, OffsetY.Value);
                _boxCollider2DOutline.UpdateOutline();
            };
            SizeX.OnValueChanged += () =>
            {
                _boxCollider2DOutline.BoxCollider.size =
                    new Vector2(SizeX.Value, _boxCollider2DOutline.BoxCollider.size.y);
                _boxCollider2DOutline.UpdateOutline();
            };
            SizeY.OnValueChanged += () =>
            {
                _boxCollider2DOutline.BoxCollider.size =
                    new Vector2(_boxCollider2DOutline.BoxCollider.size.x, SizeY.Value);
                _boxCollider2DOutline.UpdateOutline();
            };
        }

        public void CopyTo(Component targetComponent)
        {
            // if (targetComponent is BoxCollider2DComponent other)
            // {
            //     other.Sprite.Value = Sprite.Value;
            // }
            // else
            // {
            //     throw new ArgumentException("Target component must be of type NameComponent");
            // }
        }

        public void OnDestroy()
        {
            Destroy(_boxCollider2DOutline.gameObject);
        }

        public Component Copy(GameObject targetGameObject)
        {
            var component = targetGameObject.GetComponent<SpriteRendererComponent>();
            CopyTo(component);
            return component;
        }
    }
}