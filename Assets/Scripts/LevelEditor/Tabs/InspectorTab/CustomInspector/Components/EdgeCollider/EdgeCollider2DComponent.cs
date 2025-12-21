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
    public class EdgeCollider2DComponent : BaseParameterComponent
    {
        public BoolParameter isActive = new("isActive", true, Color.red);

        public ListVector2Parameter Points = new("Points", new List<Vector2>(), Color.yellow);

        public BoolParameter isDamageable = new("isDamageable", false, Color.red);
        public BoolParameter isObstacle = new("isObstacle", false, Color.red);

        private GameEventBus _eventBus;
        private EdgeColliderEditor _edgeColliderEditor;

        private bool _isUpdatingFromParameter = false;

// Методы управления флагом
        public void BeginParameterUpdate() => _isUpdatingFromParameter = true;
        public void EndParameterUpdate() => _isUpdatingFromParameter = false;

        [Inject]
        private void Construct(DiContainer container, CollidersPrefab collidersPrefab, GameEventBus eventBus)
        {
            _eventBus = eventBus;
            _edgeColliderEditor = container.InstantiatePrefab(collidersPrefab.EdgeCollider2DPrefab, transform)
                .GetComponent<EdgeColliderEditor>();

            _edgeColliderEditor.Init(Points, this);

            TransformComponent transformComponent = gameObject.GetComponent<TransformComponent>();
            transformComponent.XPosition.OnValueChanged += _edgeColliderEditor.UpdateOutline;
            transformComponent.YPosition.OnValueChanged += _edgeColliderEditor.UpdateOutline;
            transformComponent.XRotation.OnValueChanged += _edgeColliderEditor.UpdateOutline;
            transformComponent.YRotation.OnValueChanged += _edgeColliderEditor.UpdateOutline;
            transformComponent.ZRotation.OnValueChanged += _edgeColliderEditor.UpdateOutline;
            transformComponent.XScale.OnValueChanged += _edgeColliderEditor.UpdateOutline;
            transformComponent.YScale.OnValueChanged += _edgeColliderEditor.UpdateOutline;

            // Подписки на обновление
            _edgeColliderEditor.SetActiveLineRenderer(false);

            _eventBus.SubscribeTo<SelectObjectEvent>(HandleSelectObjectEvent);
            _eventBus.SubscribeTo<DeselectObjectEvent>(HandleDeselectObjectEvent);

            isActive.OnValueChanged += () => { _edgeColliderEditor.EdgeCollider2D.enabled = isActive.Value; };
            isDamageable.OnValueChanged += () =>
            {
                if (isDamageable.Value) _edgeColliderEditor.gameObject.tag = TagsStorage.IsDamageable;
                else _edgeColliderEditor.gameObject.tag = "Untagged";
            };
            isObstacle.OnValueChanged += () => { _edgeColliderEditor.EdgeCollider2D.isTrigger = !isObstacle.Value; };

            _edgeColliderEditor.EdgeCollider2D.isTrigger = !isObstacle.Value;

            Points.Value = _edgeColliderEditor.EdgeCollider2D.points.ToList();

            Points.OnValueChanged += () =>
            {
                print(Points.Value.Count);
                if (!_isUpdatingFromParameter)
                {
                    _edgeColliderEditor.ReplaceAllPoints(Points.Value);
                    _edgeColliderEditor.UpdateOutline();
                }
            };
        }

        protected override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return Points;
            yield return isActive;
            yield return isDamageable;
            yield return isObstacle;
        }

        // public override void CopyTo(Component targetComponent)
        // {
        //     print(targetComponent);
        //     if (targetComponent is EdgeCollider2DComponent other)
        //     {
        //         other.isActive.Value = isActive.Value;
        //         other.isDamageable.Value = isDamageable.Value;
        //         other.isObstacle.Value = isObstacle.Value;
        //         print(Points.Value.Count.ToString());
        //         other.Points.Value = Points.Value;
        //     }
        //     else
        //     {
        //         throw new ArgumentException("Target component must be of type NameComponent");
        //     }
        // }
        //
        private void HandleSelectObjectEvent(ref SelectObjectEvent selectObjectEvent)
        {
            _edgeColliderEditor.SetActiveLineRenderer(selectObjectEvent.Tracks.Any(i => i.sceneObject == gameObject));
            _edgeColliderEditor.UpdateOutline();
        }

        private void HandleDeselectObjectEvent(ref DeselectObjectEvent _)
        {
            _edgeColliderEditor.SetActiveLineRenderer(false);
        }

        public void OnDestroy()
        {
            TransformComponent transformComponent = gameObject.GetComponent<TransformComponent>();
            transformComponent.XPosition.OnValueChanged -= _edgeColliderEditor.UpdateOutline;
            transformComponent.YPosition.OnValueChanged -= _edgeColliderEditor.UpdateOutline;
            transformComponent.XRotation.OnValueChanged -= _edgeColliderEditor.UpdateOutline;
            transformComponent.YRotation.OnValueChanged -= _edgeColliderEditor.UpdateOutline;
            transformComponent.ZRotation.OnValueChanged -= _edgeColliderEditor.UpdateOutline;
            transformComponent.XScale.OnValueChanged -= _edgeColliderEditor.UpdateOutline;
            transformComponent.YScale.OnValueChanged -= _edgeColliderEditor.UpdateOutline;

            _eventBus.UnsubscribeFrom<SelectObjectEvent>(HandleSelectObjectEvent);
            _eventBus.UnsubscribeFrom<DeselectObjectEvent>(HandleDeselectObjectEvent);

            Destroy(_edgeColliderEditor.gameObject);
        }

        // public override Component Copy(GameObject targetGameObject)
        // {
        //     print(targetGameObject);
        //     var component = targetGameObject.AddComponent<EdgeCollider2DComponent>();
        //     CopyTo(component);
        //     return component;
        // }
    }
}