using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.EventBus.Events.EditroSceneCamera;
using TimeLine.Installers;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.General;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.EdgeColliderEditor
{
    public class PolygonColliderEditorHost : MonoBehaviour
    {
        [FormerlySerializedAs("distanceToEdge")] [SerializeField]
        float distanceToEdgeBase = 0.5f;

        [FormerlySerializedAs("distanceToCorner")] [SerializeField]
        float distanceToCornerBase = 0.3f;

        [FormerlySerializedAs("intersectingSegmentWidth")] [SerializeField]
        float intersectingSegmentWidthBase = 0.1f;

        private PolygonColliderView _view;
        private PolygonColliderController _controller;
        private IPolygonColliderModel _model;

        private GridScene _gridScene;
        private GameEventBus _gameEventBus;
        private C_EdgeColliderSelectionCube _selectionCube;
        private SceneToRawImageConverter _screenConverter;
        private C_EditColliderState _editState;
        private CameraReferences _cameraReferences;

        private float _distanceToEdgeDynamic;
        private float _distanceToCornerDynamic;
        private float _intersectingSegmentWidthDynamic;

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
            _gameEventBus.SubscribeTo((ref EditorSceneCameraUpdateViewEvent _) => CalculateDynamicFields());
        }

        internal void Init(
            ListVector2Parameter parameter, 
            PolygonCollider2DComponent edgeCollider2DComponent, 
            GameObject edgeColliderObject)
        {
            var polygonCollider2D = edgeColliderObject.GetComponent<PolygonCollider2D>();
            var lineRenderer = edgeColliderObject.GetComponent<LineRenderer>();

            // Setup Model
            _model = new PolygonColliderModel(parameter, edgeCollider2DComponent, polygonCollider2D);
            
            // Setup View
            _view = edgeColliderObject.AddComponent<PolygonColliderView>();
            _view.Initialize(edgeColliderObject.transform, lineRenderer);
            
            // Setup Controller
            _controller = new PolygonColliderController(
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
        }

        internal void UpdateOutline()
        {
            _view.UpdateOutline(_model.Points);
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

        // private void OnDestroy()
        // {
        //     _gameEventBus?.UnsubscribeFrom<EditorSceneCameraUpdateViewEvent>();
        // }
    }
}