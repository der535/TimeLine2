// using System;
// using System.Collections.Generic;
// using System.Linq;
// using EventBus;
// using TimeLine.CustomInspector.Logic;
// using TimeLine.CustomInspector.Logic.Parameter;
// using TimeLine.EdgeColliderEditor;
// using TimeLine.EventBus.Events.TrackObject;
// using TimeLine.Installers;
// using TimeLine.LevelEditor.Core;
// using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
// using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components.EdgeCollider;
// using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
// using UnityEngine;
// using Zenject;
//
// namespace TimeLine
// {
//     public class EdgeCollider2DComponent : BaseParameterComponent
//     {
//         public BoolParameter isActive = new("isActive", true, Color.red);
//
//         public ListVector2Parameter Points = new("Points", new List<Vector2>(), Color.yellow);
//         public FloatParameter edgeRadius = new("edgeRadius", 0, Color.white);
//
//         public BoolParameter isDamageable = new("isDamageable", false, Color.red);
//         public BoolParameter isObstacle = new("isObstacle", false, Color.red);
//
//         private GameEventBus _eventBus;
//         private EdgeColliderEditorHost _cEdgeColliderEditor;
//         private GameObject _edgeColliderObject;
//
//         private ActiveObjectControllerComponent _activeObjectControllerComponent;
//
//         
//         private bool _isUpdatingFromParameter;
//         private EdgeCollider2D _colliderObject;
//         private PhysicsAnchor _physicsAnchor;
//         
//         private Action<bool> _isActiveChanged;
//         
//         // Методы управления флагом
//         public void BeginParameterUpdate() => _isUpdatingFromParameter = true;
//         public void EndParameterUpdate() => _isUpdatingFromParameter = false;
//
//         [Inject]
//         private void Construct(DiContainer container, CollidersPrefab collidersPrefab, GameEventBus eventBus,
//             EdgeColliderEditorHost cEdgeColliderEditor)
//         {
//             _isActiveChanged = (bool data) =>
//             {
//                 _colliderObject.enabled = isActive.Value && data;
//             };
//             
//             _eventBus = eventBus;
//             _cEdgeColliderEditor = cEdgeColliderEditor;
//             _edgeColliderObject = container.InstantiatePrefab(collidersPrefab.edgeCollider2DPrefab);
//
//             _colliderObject = _edgeColliderObject.GetComponent<EdgeCollider2D>();
//             _physicsAnchor = _edgeColliderObject.GetComponent<PhysicsAnchor>();
//
//            _cEdgeColliderEditor.Init(Points, this, _edgeColliderObject, edgeRadius);
//            
//            _activeObjectControllerComponent = gameObject.GetComponent<ActiveObjectControllerComponent>();
//            _activeObjectControllerComponent.IsActiveChanged += _isActiveChanged;
//
//            _cEdgeColliderEditor.SetActiveLineRenderer(false);
//            
//             TransformComponent transformComponent = gameObject.GetComponent<TransformComponent>();
//             transformComponent.ChangeTransform += _cEdgeColliderEditor.UpdateOutline;
//
//             // Подписки на обновление
//             // _edgeColliderEditor.SetActiveLineRenderer(false);
//
//             _eventBus.SubscribeTo<SelectObjectEvent>(HandleSelectObjectEvent);
//             _eventBus.SubscribeTo<DeselectAllObjectEvent>(HandleDeselectObjectEvent);
//             _eventBus.SubscribeTo<DeselectObjectEvent>(HandleDeselectObjectEvent);
//
//             isActive.OnValueChanged += () => { _colliderObject.enabled = isActive.Value; };
//             isDamageable.OnValueChanged += () =>
//             {
//                 if (isDamageable.Value) _colliderObject.gameObject.tag = TagsStorage.IsDamageable;
//                 else _colliderObject.gameObject.tag = "Untagged";
//             };
//             isObstacle.OnValueChanged += () =>
//             {
//                 _colliderObject.isTrigger = !isObstacle.Value;
//             };
//
//             _colliderObject.isTrigger = !isObstacle.Value;
//
//             Points.Value = _colliderObject.points.ToList();
//
//             Points.OnValueChanged += () =>
//             {
//                 print(Points.Value.Count);
//                 if (!_isUpdatingFromParameter)
//                 {
//                     _cEdgeColliderEditor.ReplaceAllPoints(Points.Value);
//                      // _cEdgeColliderEditor.SynchronizationListVector2Parameter();
//                     _cEdgeColliderEditor.UpdateOutline();
//                 }
//             };
//             
//             _physicsAnchor.Setup(_activeObjectControllerComponent, transformComponent);
//         }
//
//         public override IEnumerable<InspectableParameter> GetParameters()
//         {
//             yield return Points;
//             yield return isActive;
//             yield return isDamageable;
//             yield return isObstacle;
//         }
//
//         private void HandleSelectObjectEvent(ref SelectObjectEvent selectObjectEvent)
//         {
//             _cEdgeColliderEditor.SetActiveLineRenderer(
//                 selectObjectEvent.Tracks.Any(i => i.sceneObject == gameObject));
//             _cEdgeColliderEditor.UpdateOutline();
//         }
//
//         private void HandleDeselectObjectEvent(ref DeselectAllObjectEvent _)
//         {
//             _cEdgeColliderEditor.SetActiveLineRenderer(false);
//         }
//
//         private void HandleDeselectObjectEvent(ref DeselectObjectEvent _)
//         {
//             _cEdgeColliderEditor.SetActiveLineRenderer(false);
//         }
//
//         public void OnDestroy()
//         {
//             _cEdgeColliderEditor.Clear();
//             _activeObjectControllerComponent.IsActiveChanged -= _isActiveChanged;
//             TransformComponent transformComponent = gameObject.GetComponent<TransformComponent>();
//             if (transformComponent.ChangeTransform != null)
//             {
//                 transformComponent.ChangeTransform -= _cEdgeColliderEditor.UpdateOutline;
//             }
//
//
//             _eventBus.UnsubscribeFrom<SelectObjectEvent>(HandleSelectObjectEvent);
//             _eventBus.UnsubscribeFrom<DeselectAllObjectEvent>(HandleDeselectObjectEvent);
//             _eventBus.UnsubscribeFrom<DeselectObjectEvent>(HandleDeselectObjectEvent);
//
//             
//             Destroy(_edgeColliderObject);
//         }
//     }
// }