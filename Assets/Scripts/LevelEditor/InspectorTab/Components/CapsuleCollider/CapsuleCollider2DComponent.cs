using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.CustomInspector.Logic;
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
    public class CapsuleCollider2DComponent : BaseParameterComponent
    {
        public BoolParameter isActive = new("isActive", true, Color.red);
        
        public FloatParameter OffsetX = new("OffsetX", 0, Color.yellow);
        public FloatParameter OffsetY = new("OffsetY", 0, Color.yellow);

        public FloatParameter SizeX = new("SizeX", 1, Color.red);
        public FloatParameter SizeY = new("SizeY", 1, Color.red);
        
        public BoolParameter isVertical = new("isVertical", true, Color.red);
        
        public BoolParameter isDamageable = new("isDamageable", false, Color.red);
        public BoolParameter isObstacle = new("isObstacle", false, Color.red);
        
        private GameEventBus _eventBus;
        private CapsuleCollider2DOutline _capsuleCollider2DOutline;
        private ActiveObjectControllerComponent _activeObjectControllerComponent;
        
        private Action<bool> _isActiveChanged;

        [Inject]
        private void Construct(DiContainer container, CollidersPrefab collidersPrefab, GameEventBus eventBus)
        {
            _isActiveChanged = (bool data) =>
            {
                _capsuleCollider2DOutline.CapsuleCollider.enabled = isActive.Value && data;
            };
            _eventBus = eventBus;
            _capsuleCollider2DOutline = container.InstantiatePrefab(collidersPrefab.CapsuleCollider2DPrefab).GetComponent<CapsuleCollider2DOutline>();

            _activeObjectControllerComponent = gameObject.GetComponent<ActiveObjectControllerComponent>();
            _activeObjectControllerComponent.IsActiveChanged += _isActiveChanged;
            
            // TransformComponent transformComponent = gameObject.GetComponent<TransformComponent>();
            // transformComponent.ChangeTransform += _capsuleCollider2DOutline.UpdateOutline;
            
            // Подписки на обновление
            _capsuleCollider2DOutline.SetActiveLineRenderer(false);
            
            _eventBus.SubscribeTo<SelectObjectEvent>(HandleSelectObjectEvent);
            _eventBus.SubscribeTo<DeselectAllObjectEvent>(HandleDeselectObjectEvent);
            _eventBus.SubscribeTo<DeselectObjectEvent>(HandleDeselectObjectEvent);
            
            isActive.OnValueChanged += () =>
            {
                _capsuleCollider2DOutline.CapsuleCollider.enabled = isActive.Value;
            };
            isDamageable.OnValueChanged += () =>
            {
                if (isDamageable.Value)  _capsuleCollider2DOutline.gameObject.tag = TagsStorage.IsDamageable;
                else  _capsuleCollider2DOutline.gameObject.tag = "Untagged";
            };
            isObstacle.OnValueChanged += () =>
            {
                _capsuleCollider2DOutline.CapsuleCollider.isTrigger = !isObstacle.Value;
            };
            
            _capsuleCollider2DOutline.CapsuleCollider.isTrigger = !isObstacle.Value;
            
            
            
            OffsetX.Value = _capsuleCollider2DOutline.CapsuleCollider.offset.x;
            OffsetY.Value = _capsuleCollider2DOutline.CapsuleCollider.offset.y;
            SizeX.Value = _capsuleCollider2DOutline.CapsuleCollider.size.x;
            SizeY.Value = _capsuleCollider2DOutline.CapsuleCollider.size.y;
            isVertical.Value = _capsuleCollider2DOutline.CapsuleCollider.direction == CapsuleDirection2D.Vertical;

            OffsetX.OnValueChanged += () =>
            {
                _capsuleCollider2DOutline.CapsuleCollider.offset =
                    new Vector2(OffsetX.Value, _capsuleCollider2DOutline.CapsuleCollider.offset.y);
                _capsuleCollider2DOutline.UpdateOutline();
            };
            OffsetY.OnValueChanged += () =>
            {
                _capsuleCollider2DOutline.CapsuleCollider.offset =
                    new Vector2(_capsuleCollider2DOutline.CapsuleCollider.offset.x, OffsetY.Value);
                _capsuleCollider2DOutline.UpdateOutline();
            };
            SizeX.OnValueChanged += () =>
            {
                _capsuleCollider2DOutline.CapsuleCollider.size =
                    new Vector2(SizeX.Value, _capsuleCollider2DOutline.CapsuleCollider.size.y);
                _capsuleCollider2DOutline.UpdateOutline();
            };
            SizeY.OnValueChanged += () =>
            {
                _capsuleCollider2DOutline.CapsuleCollider.size =
                    new Vector2(_capsuleCollider2DOutline.CapsuleCollider.size.x, SizeY.Value);
                _capsuleCollider2DOutline.UpdateOutline();
            };
            isVertical.OnValueChanged += () =>
            {
                _capsuleCollider2DOutline.CapsuleCollider.direction = isVertical.Value
                    ? CapsuleDirection2D.Vertical
                    : CapsuleDirection2D.Horizontal;
                _capsuleCollider2DOutline.UpdateOutline();
            };
            // _capsuleCollider2DOutline.Setup(_activeObjectControllerComponent, transformComponent);
        }
        public override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return OffsetX;
            yield return OffsetY;
            yield return SizeX;
            yield return SizeY;
            yield return isActive;
            yield return isVertical;
            yield return isDamageable;
            yield return isObstacle;
        }
        
        private void HandleSelectObjectEvent(ref SelectObjectEvent selectObjectEvent)
        {
            _capsuleCollider2DOutline.SetActiveLineRenderer(selectObjectEvent.Tracks.Any(i => i.sceneObject == gameObject));
            _capsuleCollider2DOutline.UpdateOutline();
        }

        private void HandleDeselectObjectEvent(ref DeselectAllObjectEvent _)
        {
            _capsuleCollider2DOutline.SetActiveLineRenderer(false);
        }
                private void HandleDeselectObjectEvent(ref DeselectObjectEvent _)
                {
                    _capsuleCollider2DOutline.SetActiveLineRenderer(false);
                }
        

        public void OnDestroy()
        {
            _activeObjectControllerComponent.IsActiveChanged -= _isActiveChanged;
            // TransformComponent transformComponent = gameObject.GetComponent<TransformComponent>();
            // transformComponent.ChangeTransform -= _capsuleCollider2DOutline.UpdateOutline;
            
            _eventBus.UnsubscribeFrom<SelectObjectEvent>(HandleSelectObjectEvent);
            _eventBus.UnsubscribeFrom<DeselectAllObjectEvent>(HandleDeselectObjectEvent);
            _eventBus.UnsubscribeFrom<DeselectObjectEvent>(HandleDeselectObjectEvent);

            
            Destroy(_capsuleCollider2DOutline.gameObject);
        }

        // public override Component Copy(GameObject targetGameObject)
        // {
        //     var component = targetGameObject.AddComponent<BoxCollider2DComponent>();
        //     CopyTo(component);
        //     return component;
        // }
    }
}