using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class BoxCollider2DComponent : BaseParameterComponent
    {
        public BoolParameter isActive = new("isActive", true, Color.red);

        public FloatParameter OffsetX = new("OffsetX", 0, Color.yellow);
        public FloatParameter OffsetY = new("OffsetY", 0, Color.yellow);

        public FloatParameter SizeX = new("SizeX", 1, Color.red);
        public FloatParameter SizeY = new("SizeY", 1, Color.red);

        public BoolParameter isDamageable = new("isDamageable", false, Color.red);
        public BoolParameter isObstacle = new("isObstacle", false, Color.red);

        private GameEventBus _eventBus;
        private BoxCollider2DOutline _boxCollider2DOutline;
        private ActiveObjectControllerComponent _activeObjectControllerComponent;

        private Action<bool> _isActiveChanged;

        [Inject]
        private void Construct(DiContainer container, CollidersPrefab collidersPrefab, GameEventBus eventBus)
        {
            _isActiveChanged = (bool data) => { _boxCollider2DOutline.BoxCollider.enabled = isActive.Value && data; };
            _eventBus = eventBus;
            _boxCollider2DOutline = container.InstantiatePrefab(collidersPrefab.BoxCollider2DPrefab)
                .GetComponent<BoxCollider2DOutline>();

            _activeObjectControllerComponent = gameObject.GetComponent<ActiveObjectControllerComponent>();
            _activeObjectControllerComponent.IsActiveChanged += _isActiveChanged;

            TransformComponent transformComponent = gameObject.GetComponent<TransformComponent>();
            transformComponent.ChangeTransform += _boxCollider2DOutline.UpdateOutline;

            // Подписки на обновление
            _boxCollider2DOutline.SetActiveLineRenderer(false);

            _eventBus.SubscribeTo<SelectObjectEvent>(HandleSelectObjectEvent);
            _eventBus.SubscribeTo<DeselectAllObjectEvent>(HandleDeselectObjectEvent);
            _eventBus.SubscribeTo<DeselectObjectEvent>(HandleDeselectObjectEvent);

            isActive.OnValueChanged += () => { _boxCollider2DOutline.BoxCollider.enabled = isActive.Value; };
            isDamageable.OnValueChanged += () =>
            {
                if (isDamageable.Value) _boxCollider2DOutline.gameObject.tag = TagsStorage.IsDamageable;
                else _boxCollider2DOutline.gameObject.tag = "Untagged";
            };
            isObstacle.OnValueChanged += () => { _boxCollider2DOutline.BoxCollider.isTrigger = !isObstacle.Value; };

            _boxCollider2DOutline.BoxCollider.isTrigger = !isObstacle.Value;


            OffsetX.Value = _boxCollider2DOutline.BoxCollider.offset.x;
            OffsetY.Value = _boxCollider2DOutline.BoxCollider.offset.y;
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

            _boxCollider2DOutline.Setup(_activeObjectControllerComponent, transformComponent);
        }

        protected override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return OffsetX;
            yield return OffsetY;
            yield return SizeX;
            yield return SizeY;
            yield return isActive;
            yield return isDamageable;
            yield return isObstacle;
        }

        private void HandleSelectObjectEvent(ref SelectObjectEvent selectObjectEvent)
        {
            _boxCollider2DOutline.SetActiveLineRenderer(selectObjectEvent.Tracks.Any(i => i.sceneObject == gameObject));
            _boxCollider2DOutline.UpdateOutline();
        }

        private void HandleDeselectObjectEvent(ref DeselectAllObjectEvent _)
        {
            _boxCollider2DOutline.SetActiveLineRenderer(false);
        }

        private void HandleDeselectObjectEvent(ref DeselectObjectEvent _)
        {
            _boxCollider2DOutline.SetActiveLineRenderer(false);
        }

        public void OnDestroy()
        {
            _activeObjectControllerComponent.IsActiveChanged -= _isActiveChanged;
            TransformComponent transformComponent = gameObject.GetComponent<TransformComponent>();

            if (transformComponent.ChangeTransform != null)
            {
                transformComponent.ChangeTransform -= _boxCollider2DOutline.UpdateOutline;
            }

            _eventBus.UnsubscribeFrom<SelectObjectEvent>(HandleSelectObjectEvent);
            _eventBus.UnsubscribeFrom<DeselectAllObjectEvent>(HandleDeselectObjectEvent);
            _eventBus.UnsubscribeFrom<DeselectObjectEvent>(HandleDeselectObjectEvent);


            if(_boxCollider2DOutline != null)
                Destroy(_boxCollider2DOutline?.gameObject);
        }
    }
}