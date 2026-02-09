using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.SpriteLoader;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class SpriteRendererComponent : BaseParameterComponent
    {
        public SpriteParameter Sprite = new("Sprite", null, Color.magenta);
        [Space] public IntParameter OrderInLayer = new("OrderInLayer", 0, Color.grey);
        [Space] public BoolParameter InvertX = new("InvertX", false, Color.grey);
        public BoolParameter InvertY = new("InvertY", false, Color.grey);
        [Space] public ColorParameter SpriteColor = new("SpriteColor", Color.white, Color.white);

        private SelectSpriteController _selectSpriteController;

        private SpriteRenderer _spriteRenderer;

        // private PixelPerfectClick _pixelPerfectClick;
        private DiContainer _container;
        private CustomSpriteStorage _customSpriteStorage;
        private ActiveObjectControllerComponent activeObjectController;
        private TrackObjectStorage _trackObjectStorage;

        private Action<bool> Active;

        [Inject]
        private void Construct(SelectSpriteController selectSpriteController, DiContainer container,
            CustomSpriteStorage customSpriteStorage, TrackObjectStorage trackObjectStorage)
        {
            _selectSpriteController = selectSpriteController;
            _container = container;
            _customSpriteStorage = customSpriteStorage;

            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

            if (_spriteRenderer == null)
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            Sprite.OnValueChanged += () => { _spriteRenderer.sprite = Sprite.Value; };
            OrderInLayer.OnValueChanged += () => { _spriteRenderer.sortingOrder = OrderInLayer.Value; };
            InvertX.OnValueChanged += () => { _spriteRenderer.flipX = InvertX.Value; };
            InvertY.OnValueChanged += () => { _spriteRenderer.flipY = InvertY.Value; };
            SpriteColor.OnValueChanged += () => { _spriteRenderer.color = SpriteColor.Value; };

            _trackObjectStorage = trackObjectStorage;
            

        }

        private void Start()
        {
            activeObjectController = gameObject.GetComponent<SceneObjectLink>().trackObjectData.activeObjectController;
            Active += active =>
            {
                _spriteRenderer.enabled = active;
            };

            activeObjectController.IsActiveChanged += Active;

            
            _selectSpriteController.CheckSpriteRendererAndAdd(Sprite);
        }


        private void OnDestroy()
        {
            _customSpriteStorage.CheckAndRemoveSpriteRenderer(Sprite);
            activeObjectController.IsActiveChanged -= Active;

            Destroy(_spriteRenderer);
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