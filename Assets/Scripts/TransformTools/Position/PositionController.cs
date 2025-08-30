using System;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PositionController : MonoBehaviour
    {
        [SerializeField] private CoordinateSystem coordinateSystem;
        [SerializeField] private PositionTool positionTool;
        [SerializeField] private Camera camera;
        [SerializeField] private GridScene gridScene;
        
        [SerializeField] private RectTransform _rectTransformPositionTool;
        private MainObjects _mainObjects;
        private GameEventBus _gameEventBus;
        
        private TransformComponent _transformComponent;
        
        private Action<Vector2> _objectFollowingTool;
        private Action _toolFollowingObject;
        
        [Inject]
        private void Construct(GameEventBus eventBus, MainObjects mainObjects)
        {
            _gameEventBus = eventBus;
            _mainObjects = mainObjects;
        }
        private void Start()
        {
            // _gameEventBus.SubscribeTo<SelectSceneObject>(((ref SelectSceneObject data) => SelectObject(data.GameObject)));
            _gameEventBus.SubscribeTo(((ref SelectObjectEvent data) => SelectObject(data.Track.sceneObject)));
            coordinateSystem.OnCoordinateChanged += b => _rectTransformPositionTool.rotation = Quaternion.Euler(0f, 0f, b ? 0f : _transformComponent.ZRotation.Value);
        }
        private void SelectObject(GameObject data)
        {
            print("SelectObject");
            if (_transformComponent)
            {
                _transformComponent.XPosition.OnValueChanged -= _toolFollowingObject;
                _transformComponent.YPosition.OnValueChanged -= _toolFollowingObject;
            }
            
            _transformComponent = data.gameObject.GetComponent<TransformComponent>();
            _rectTransformPositionTool.anchoredPosition = camera.WorldToScreenPoint(data.transform.position);
            _rectTransformPositionTool.anchoredPosition -= _mainObjects.CanvasRectTransform.sizeDelta / 2;

            _rectTransformPositionTool.rotation = Quaternion.Euler(0f, 0f, coordinateSystem.IsGlobal ? 0f : _transformComponent.ZRotation.Value);

            if(positionTool.OnChangePosition != null)
                positionTool.OnChangePosition -= _objectFollowingTool;
            
            float rotationAngle = -_rectTransformPositionTool.localEulerAngles.z;
            
            _objectFollowingTool = vector2 =>
            {
                Vector2 convertedPosition =
                    camera.ScreenToWorldPoint(vector2 + _mainObjects.CanvasRectTransform.sizeDelta / 2);
                
                float rotationAngle = -_rectTransformPositionTool.localEulerAngles.z;
                
                Vector2 calculatedPosition = gridScene.PositionFloatSnapToGrid(convertedPosition, Quaternion.Euler(0, 0, rotationAngle));

                _transformComponent.XPosition.Value = calculatedPosition.x;
                _transformComponent.YPosition.Value = calculatedPosition.y;
            };
            positionTool.OnChangePosition += _objectFollowingTool;
            
            _toolFollowingObject = () =>
            {
                Vector2 position = camera.WorldToScreenPoint( new Vector2(_transformComponent.XPosition.Value, _transformComponent.YPosition.Value));
                position -= _mainObjects.CanvasRectTransform.sizeDelta / 2;
                _rectTransformPositionTool.anchoredPosition = position;
            };
            
            _transformComponent.XPosition.OnValueChanged += _toolFollowingObject;
            _transformComponent.YPosition.OnValueChanged += _toolFollowingObject;
        }
    }
}
