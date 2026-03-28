using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.EventBus.Events.EditroSceneCamera;
using TimeLine.Installers;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.InspectorTab.Components.EdgeCollider;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.EdgeColliderEditor
{
    public class EdgeColliderEditorHost : MonoBehaviour
    {
        [FormerlySerializedAs("distanceToEdge")] [SerializeField]
        float distanceToEdgeBase = 0.5f;

        [FormerlySerializedAs("distanceToCorner")] [SerializeField]
        float distanceToCornerBase = 0.3f;

        [FormerlySerializedAs("intersectingSegmentWidth")] [SerializeField]
        float intersectingSegmentWidthBase = 0.1f;

        private EdgeColliderView _view;
        private EdgeColliderController _controller;
        private IEdgeColliderModel _model;

        private GridScene _gridScene;
        private GameEventBus _gameEventBus;
        private C_EdgeColliderSelectionCube _selectionCube;
        private SceneToRawImageConverter _screenConverter;
        private C_EditColliderState _editState;
        private CameraReferences _cameraReferences;

        private float _distanceToEdgeDynamic;
        private float _distanceToCornerDynamic;
        private float _intersectingSegmentWidthDynamic;

        private EventBinder _eventBinder;

        [Inject]
        void Construct(
            GridScene gridScene,
            GameEventBus eventBus,
            C_EdgeColliderSelectionCube selectionCube,
            SceneToRawImageConverter screenConverter,
            C_EditColliderState editState,
            CameraReferences cameraReferences)
        {
            _gridScene = gridScene;
            _gameEventBus = eventBus;
            _selectionCube = selectionCube;
            _screenConverter = screenConverter;
            _editState = editState;
            _cameraReferences = cameraReferences;
        }

        private void Start()
        {
            _eventBinder = new EventBinder();
            _eventBinder.Add(_gameEventBus, (ref EditorSceneCameraUpdateViewEvent _) => CalculateDynamicFields());
        }

        internal void Init(
            ListVector2Parameter parameter, 
            // EdgeCollider2DComponent edgeCollider2DComponent, 
            GameObject edgeColliderObject,
            FloatParameter edgeRadius)
        {
            var edgeCollider2D = edgeColliderObject.GetComponent<EdgeCollider2D>();
            var lineRenderer = edgeColliderObject.GetComponent<LineRenderer>();

            // Setup Model
            // _model = new EdgeColliderModel(parameter, edgeCollider2DComponent, edgeCollider2D, edgeRadius);
            
            // Setup View
            _view = edgeColliderObject.AddComponent<EdgeColliderView>();
            _view.Initialize(edgeColliderObject.transform, lineRenderer);
            
            // Setup Controller
            _controller = new EdgeColliderController(
                _model,
                _view,
                _gridScene,
                _selectionCube,
                _screenConverter,
                _editState,
                _cameraReferences
            );
            
            // Initial visual update
            CalculateDynamicFields();
            _view.UpdateOutline(_model.Points);
            SetActiveLineRenderer(true);
        }

        internal void Clear()
        {
            _model = null;
            _view = null;
            _controller = null;
        }

        internal void SetActiveLineRenderer(bool active)
        {
            if (_view?.OutlineRenderer != null)
                _view.OutlineRenderer.enabled = active;
        }

        internal void CalculateDynamicFields()
        {
            float orthoSize = _cameraReferences.editSceneCamera.orthographicSize;
            _distanceToEdgeDynamic = distanceToEdgeBase * orthoSize;
            _distanceToCornerDynamic = distanceToCornerBase * orthoSize;
            _intersectingSegmentWidthDynamic = intersectingSegmentWidthBase * orthoSize;

            if (_view == null) return;

            _view.DistanceToEdgeDynamic = _distanceToEdgeDynamic;
            _view.DistanceToCornerDynamic = _distanceToCornerDynamic;
            _view.IntersectingSegmentWidthDynamic = _intersectingSegmentWidthDynamic;

            if (_view.OutlineRenderer != null)
            {
                _view.OutlineRenderer.startWidth = _intersectingSegmentWidthDynamic;
                _view.OutlineRenderer.endWidth = _intersectingSegmentWidthDynamic;
            }

            foreach (var item in _view.RadiusVisualizers)
            {
                item.startWidth = _intersectingSegmentWidthDynamic;
                item.endWidth = _intersectingSegmentWidthDynamic;
            }
            
            foreach (var item in _view.EdgeConnectionVisualizers)
            {
                item.startWidth = _intersectingSegmentWidthDynamic;
                item.endWidth = _intersectingSegmentWidthDynamic;
            }
        }

        internal void UpdateOutline()
        {
            _view?.UpdateOutline(_model.Points);
        }
        

        public void ReplaceAllPoints(List<Vector2> worldPoints)
        {
            _controller?.ApplyPoints(worldPoints);
            _view?.UpdateOutline(worldPoints);
        }

        private void Update()
        {
            _controller?.Update();
        }

        private void OnDestroy()
        {
            _eventBinder.Dispose();
        }
    }
}