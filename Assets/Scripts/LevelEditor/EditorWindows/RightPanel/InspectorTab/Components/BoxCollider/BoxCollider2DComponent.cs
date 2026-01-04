using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.EventBus.Events.TrackObject;
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

        [Inject]
        private void Construct(DiContainer container, CollidersPrefab collidersPrefab, GameEventBus eventBus)
        {
            _eventBus = eventBus;
            _boxCollider2DOutline = container.InstantiatePrefab(collidersPrefab.BoxCollider2DPrefab, transform).GetComponent<BoxCollider2DOutline>();

            TransformComponent transformComponent = gameObject.GetComponent<TransformComponent>();
            transformComponent.XPosition.OnValueChanged += _boxCollider2DOutline.UpdateOutline;
            transformComponent.YPosition.OnValueChanged += _boxCollider2DOutline.UpdateOutline;
            transformComponent.XRotation.OnValueChanged += _boxCollider2DOutline.UpdateOutline;
            transformComponent.YRotation.OnValueChanged += _boxCollider2DOutline.UpdateOutline;
            transformComponent.ZRotation.OnValueChanged += _boxCollider2DOutline.UpdateOutline;
            transformComponent.XScale.OnValueChanged += _boxCollider2DOutline.UpdateOutline;
            transformComponent.YScale.OnValueChanged += _boxCollider2DOutline.UpdateOutline;
            
            // Подписки на обновление
            _boxCollider2DOutline.SetActiveLineRenderer(false);
            
            _eventBus.SubscribeTo<SelectObjectEvent>(HandleSelectObjectEvent);
            _eventBus.SubscribeTo<DeselectAllObjectEvent>(HandleDeselectObjectEvent);
            _eventBus.SubscribeTo<DeselectObjectEvent>(HandleDeselectObjectEvent);
            
            isActive.OnValueChanged += () =>
            {
                _boxCollider2DOutline.BoxCollider.enabled = isActive.Value;
            };
            isDamageable.OnValueChanged += () =>
            {
                if (isDamageable.Value)  _boxCollider2DOutline.gameObject.tag = TagsStorage.IsDamageable;
                else  _boxCollider2DOutline.gameObject.tag = "Untagged";
            };
            isObstacle.OnValueChanged += () =>
            {
                _boxCollider2DOutline.BoxCollider.isTrigger = !isObstacle.Value;
            };
            
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

        // public override void CopyTo(Component targetComponent)
        // {
        //     if (targetComponent is BoxCollider2DComponent other)
        //     {
        //         other.OffsetX.Value = _boxCollider2DOutline.BoxCollider.offset.x;
        //         other.OffsetY.Value = _boxCollider2DOutline.BoxCollider.offset.y;
        //         other.SizeX.Value = _boxCollider2DOutline.BoxCollider.size.x;
        //         other.SizeY.Value = _boxCollider2DOutline.BoxCollider.size.y;
        //     }
        //     else
        //     {
        //         throw new ArgumentException("Target component must be of type NameComponent");
        //     }
        // }
        
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
            TransformComponent transformComponent = gameObject.GetComponent<TransformComponent>();
            transformComponent.XPosition.OnValueChanged -= _boxCollider2DOutline.UpdateOutline;
            transformComponent.YPosition.OnValueChanged -= _boxCollider2DOutline.UpdateOutline;
            transformComponent.XRotation.OnValueChanged -= _boxCollider2DOutline.UpdateOutline;
            transformComponent.YRotation.OnValueChanged -= _boxCollider2DOutline.UpdateOutline;
            transformComponent.ZRotation.OnValueChanged -= _boxCollider2DOutline.UpdateOutline;
            transformComponent.XScale.OnValueChanged -= _boxCollider2DOutline.UpdateOutline;
            transformComponent.YScale.OnValueChanged -= _boxCollider2DOutline.UpdateOutline;
            
            _eventBus.UnsubscribeFrom<SelectObjectEvent>(HandleSelectObjectEvent);
            _eventBus.UnsubscribeFrom<DeselectAllObjectEvent>(HandleDeselectObjectEvent);
            _eventBus.UnsubscribeFrom<DeselectObjectEvent>(HandleDeselectObjectEvent);

            
            Destroy(_boxCollider2DOutline.gameObject);
        }
        //
        // public override Component Copy(GameObject targetGameObject)
        // {
        //     var component = targetGameObject.AddComponent<BoxCollider2DComponent>();
        //     CopyTo(component);
        //     return component;
        // }
    }
}