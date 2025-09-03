using System;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using TimeLine.TimeLine;
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
        [SerializeField] private Canvas canvas;
        
        [SerializeField] private RectTransform _rectTransformPositionTool;
        private MainObjects _mainObjects;
        private GameEventBus _gameEventBus;
        
        private TransformComponent _transformComponent;
        
        private bool _isUpdatingFromTool = false;
        private bool _isUpdatingFromObject = false;
        
        [Inject]
        private void Construct(GameEventBus eventBus, MainObjects mainObjects)
        {
            _gameEventBus = eventBus;
            _mainObjects = mainObjects;
        }
        
        private void Start()
        {
            _gameEventBus.SubscribeTo(((ref SelectObjectEvent data) => SelectObject(data.Track.sceneObject)));
            coordinateSystem.OnCoordinateChanged += b => _rectTransformPositionTool.rotation = Quaternion.Euler(0f, 0f, b ? 0f : _transformComponent.ZRotation.Value);
        }
        
        private void SelectObject(GameObject data)
        {
            // print("SelectObject");
            if (_transformComponent)
            {
                _transformComponent.XPosition.OnValueChanged -= UpdateToolFromObject;
                _transformComponent.YPosition.OnValueChanged -= UpdateToolFromObject;
            }
            
            _transformComponent = data.gameObject.GetComponent<TransformComponent>();
            
            Vector2 snappedWorldPosition = gridScene.PositionFloatSnapToGrid(
                new Vector2(_transformComponent.XPosition.Value, _transformComponent.YPosition.Value), 
                Quaternion.identity
            );
            
            _transformComponent.XPosition.Value = snappedWorldPosition.x;
            _transformComponent.YPosition.Value = snappedWorldPosition.y;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(  
                _mainObjects.ToolRectTransform,
                camera.WorldToScreenPoint(snappedWorldPosition),
                camera,
                out var localPoint
            );
            
            _rectTransformPositionTool.anchoredPosition = localPoint;
            _rectTransformPositionTool.rotation = Quaternion.Euler(0f, 0f, coordinateSystem.IsGlobal ? 0f : _transformComponent.ZRotation.Value);
            
            positionTool.OnChangePosition -= UpdateObjectFromTool;
            positionTool.OnChangePosition += UpdateObjectFromTool;
            
            _transformComponent.XPosition.OnValueChanged += UpdateToolFromObject;
            _transformComponent.YPosition.OnValueChanged += UpdateToolFromObject;
        }
        
        private void UpdateObjectFromTool(RectTransform toolTransform)
        {
            if (_isUpdatingFromObject) return;
            _isUpdatingFromTool = true;
            
            Vector2 convertedPosition = TimeLineConverter.ConvertAnchorToWorldPosition(toolTransform, canvas);
            
            float rotationAngle = -toolTransform.localEulerAngles.z;
            Quaternion inverseRotation = Quaternion.Euler(0, 0, rotationAngle);
            
            Vector2 snappedPosition = gridScene.PositionFloatSnapToGrid(convertedPosition, inverseRotation);
            
            Vector2 localSnappedPosition = Quaternion.Inverse(inverseRotation) * (snappedPosition - (Vector2)_transformComponent.transform.position);
            localSnappedPosition += new Vector2(_transformComponent.XPosition.Value, _transformComponent.YPosition.Value);
            
            _transformComponent.XPosition.Value = localSnappedPosition.x;
            _transformComponent.YPosition.Value = localSnappedPosition.y;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(  
                _mainObjects.ToolRectTransform,
                camera.WorldToScreenPoint(snappedPosition),
                camera,
                out var snappedLocalPoint
            );
            _rectTransformPositionTool.anchoredPosition = snappedLocalPoint;
            
            _isUpdatingFromTool = false;
        }
        
        private void UpdateToolFromObject()
        {
            if (_isUpdatingFromTool) return;
            _isUpdatingFromObject = true;
            
            float rotationAngle = -_rectTransformPositionTool.localEulerAngles.z;
            Quaternion inverseRotation = Quaternion.Euler(0, 0, rotationAngle);
            
            Vector2 worldPosition = new Vector2(_transformComponent.XPosition.Value, _transformComponent.YPosition.Value);
            Vector2 snappedWorldPosition = gridScene.PositionFloatSnapToGrid(worldPosition, inverseRotation);
            
            Vector2 localSnappedPosition = Quaternion.Inverse(inverseRotation) * (snappedWorldPosition - (Vector2)_transformComponent.transform.position);
            localSnappedPosition += new Vector2(_transformComponent.XPosition.Value, _transformComponent.YPosition.Value);
            
            _transformComponent.XPosition.Value = localSnappedPosition.x;
            _transformComponent.YPosition.Value = localSnappedPosition.y;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(  
                _mainObjects.ToolRectTransform,
                camera.WorldToScreenPoint(snappedWorldPosition),
                camera,
                out var localPoint
            );
            _rectTransformPositionTool.anchoredPosition = localPoint;
            
            _isUpdatingFromObject = false;
        }
    }
}