using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.CustomInspector.Logic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class CircleCollider2DComponent : BaseParameterComponent
    {
        public BoolParameter isActive = new("isActive", true, Color.red);
        
        public FloatParameter OffsetX = new("OffsetX", 0, Color.yellow);
        public FloatParameter OffsetY = new("OffsetY", 0, Color.yellow);

        public FloatParameter Radius = new("SizeX", 1, Color.red);
        
        public BoolParameter isDamageable = new("isDamageable", false, Color.red);
        public BoolParameter isObstacle = new("isObstacle", false, Color.red);
        
        private GameEventBus _eventBus;
        private CircleCollider2DOutline _circleCollider2DOutline;

        [Inject]
        private void Construct(DiContainer container, CollidersPrefab collidersPrefab, GameEventBus eventBus)
        {
            _eventBus = eventBus;
            _circleCollider2DOutline = container.InstantiatePrefab(collidersPrefab.CircleCollider2DPrefab, transform).GetComponent<CircleCollider2DOutline>();

            TransformComponent transformComponent = gameObject.GetComponent<TransformComponent>();
            transformComponent.XPosition.OnValueChanged += _circleCollider2DOutline.UpdateOutline;
            transformComponent.YPosition.OnValueChanged += _circleCollider2DOutline.UpdateOutline;
            transformComponent.XRotation.OnValueChanged += _circleCollider2DOutline.UpdateOutline;
            transformComponent.YRotation.OnValueChanged += _circleCollider2DOutline.UpdateOutline;
            transformComponent.ZRotation.OnValueChanged += _circleCollider2DOutline.UpdateOutline;
            transformComponent.XScale.OnValueChanged += _circleCollider2DOutline.UpdateOutline;
            transformComponent.YScale.OnValueChanged += _circleCollider2DOutline.UpdateOutline;
            
            // Подписки на обновление
            _circleCollider2DOutline.SetActiveLineRenderer(false);
            
            _eventBus.SubscribeTo<SelectObjectEvent>(HandleSelectObjectEvent);
            _eventBus.SubscribeTo<DeselectObjectEvent>(HandleDeselectObjectEvent);
            
            isActive.OnValueChanged += () =>
            {
                _circleCollider2DOutline.CircleCollider.enabled = isActive.Value;
            };
            isDamageable.OnValueChanged += () =>
            {
                if (isDamageable.Value)  _circleCollider2DOutline.gameObject.tag = TagsStorage.IsDamageable;
                else  _circleCollider2DOutline.gameObject.tag = "Untagged";
            };
            isObstacle.OnValueChanged += () =>
            {
                _circleCollider2DOutline.CircleCollider.isTrigger = !isObstacle.Value;
            };
            
            _circleCollider2DOutline.CircleCollider.isTrigger = !isObstacle.Value;
            
            
            
            OffsetX.Value = _circleCollider2DOutline.CircleCollider.offset.x;
            OffsetY.Value = _circleCollider2DOutline.CircleCollider.offset.y;
            Radius.Value = _circleCollider2DOutline.CircleCollider.radius;

            OffsetX.OnValueChanged += () =>
            {
                _circleCollider2DOutline.CircleCollider.offset =
                    new Vector2(OffsetX.Value, _circleCollider2DOutline.CircleCollider.offset.y);
                _circleCollider2DOutline.UpdateOutline();
            };
            OffsetY.OnValueChanged += () =>
            {
                _circleCollider2DOutline.CircleCollider.offset =
                    new Vector2(_circleCollider2DOutline.CircleCollider.offset.x, OffsetY.Value);
                _circleCollider2DOutline.UpdateOutline();
            };
            Radius.OnValueChanged += () =>
            {
                _circleCollider2DOutline.CircleCollider.radius = Radius.Value;
                _circleCollider2DOutline.UpdateOutline();
            };
        }
        protected override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return OffsetX;
            yield return OffsetY;
            yield return Radius;
            yield return isActive;
            yield return isDamageable;
            yield return isObstacle;
        }

        // public override void CopyTo(Component targetComponent)
        // {
        //     if (targetComponent is CircleCollider2DComponent other)
        //     {
        //         other.OffsetX.Value = _circleCollider2DOutline.CircleCollider.offset.x;
        //         other.OffsetY.Value = _circleCollider2DOutline.CircleCollider.offset.y;
        //         other.Radius.Value = _circleCollider2DOutline.CircleCollider.radius;
        //     }
        //     else
        //     {
        //         throw new ArgumentException("Target component must be of type NameComponent");
        //     }
        // }
        
        private void HandleSelectObjectEvent(ref SelectObjectEvent selectObjectEvent)
        {
            _circleCollider2DOutline.SetActiveLineRenderer(selectObjectEvent.Tracks.Any(i => i.sceneObject == gameObject));
            _circleCollider2DOutline.UpdateOutline();
        }

        private void HandleDeselectObjectEvent(ref DeselectObjectEvent _)
        {
            _circleCollider2DOutline.SetActiveLineRenderer(false);
        }

        public void OnDestroy()
        {
            TransformComponent transformComponent = gameObject.GetComponent<TransformComponent>();
            transformComponent.XPosition.OnValueChanged -= _circleCollider2DOutline.UpdateOutline;
            transformComponent.YPosition.OnValueChanged -= _circleCollider2DOutline.UpdateOutline;
            transformComponent.XRotation.OnValueChanged -= _circleCollider2DOutline.UpdateOutline;
            transformComponent.YRotation.OnValueChanged -= _circleCollider2DOutline.UpdateOutline;
            transformComponent.ZRotation.OnValueChanged -= _circleCollider2DOutline.UpdateOutline;
            transformComponent.XScale.OnValueChanged -= _circleCollider2DOutline.UpdateOutline;
            transformComponent.YScale.OnValueChanged -= _circleCollider2DOutline.UpdateOutline;
            
            _eventBus.UnsubscribeFrom<SelectObjectEvent>(HandleSelectObjectEvent);
            _eventBus.UnsubscribeFrom<DeselectObjectEvent>(HandleDeselectObjectEvent);
            
            Destroy(_circleCollider2DOutline.gameObject);
        }

        // public override Component Copy(GameObject targetGameObject)
        // {
        //     var component = targetGameObject.AddComponent<BoxCollider2DComponent>();
        //     CopyTo(component);
        //     return component;
        // }
    }
}