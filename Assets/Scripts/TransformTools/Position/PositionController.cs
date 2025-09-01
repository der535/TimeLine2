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
        
        private Action<RectTransform> _objectFollowingTool;
        private Action _toolFollowingObject;
        
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
            print("SelectObject");
            if (_transformComponent)
            {
                _transformComponent.XPosition.OnValueChanged -= _toolFollowingObject;
                _transformComponent.YPosition.OnValueChanged -= _toolFollowingObject;
            }
            
            _transformComponent = data.gameObject.GetComponent<TransformComponent>();
            
            // Получаем текущую позицию объекта с учетом сетки
            Vector2 snappedWorldPosition = gridScene.PositionFloatSnapToGrid(
                new Vector2(_transformComponent.XPosition.Value, _transformComponent.YPosition.Value), 
                Quaternion.identity
            );
            
            // Обновляем позицию объекта к сетке
            _transformComponent.XPosition.Value = snappedWorldPosition.x;
            _transformComponent.YPosition.Value = snappedWorldPosition.y;
            
            // Конвертируем мировые координаты в UI координаты
            RectTransformUtility.ScreenPointToLocalPointInRectangle(  
                _mainObjects.ToolRectTransform,
                camera.WorldToScreenPoint(snappedWorldPosition),
                camera,
                out var localPoint
            );
            
            _rectTransformPositionTool.anchoredPosition = localPoint;
            _rectTransformPositionTool.rotation = Quaternion.Euler(0f, 0f, coordinateSystem.IsGlobal ? 0f : _transformComponent.ZRotation.Value);

            if(positionTool.OnChangePosition != null)
                positionTool.OnChangePosition -= _objectFollowingTool;
            
            _objectFollowingTool = vector2 =>
            {
                Vector2 convertedPosition = TimeLineConverter.ConvertAnchorToWorldPosition(vector2, canvas);
                
                // Применяем привязку к сетке
                Vector2 calculatedPosition = gridScene.PositionFloatSnapToGrid(convertedPosition, Quaternion.identity);

                // Обновляем позицию объекта
                _transformComponent.XPosition.Value = calculatedPosition.x;
                _transformComponent.YPosition.Value = calculatedPosition.y;
                
                // Немедленно обновляем позицию инструмента к сетке
                RectTransformUtility.ScreenPointToLocalPointInRectangle(  
                    _mainObjects.ToolRectTransform,
                    camera.WorldToScreenPoint(calculatedPosition),
                    camera,
                    out var snappedLocalPoint
                );
                _rectTransformPositionTool.anchoredPosition = snappedLocalPoint;
            };
            positionTool.OnChangePosition += _objectFollowingTool;
            
            _toolFollowingObject = () =>
            {
                // Получаем текущую позицию объекта с учетом сетки
                Vector2 snappedWorldPosition = gridScene.PositionFloatSnapToGrid(
                    new Vector2(_transformComponent.XPosition.Value, _transformComponent.YPosition.Value), 
                    Quaternion.identity
                );
                
                // Обновляем позицию объекта к сетке (на случай, если значение было изменено извне)
                _transformComponent.XPosition.Value = snappedWorldPosition.x;
                _transformComponent.YPosition.Value = snappedWorldPosition.y;
                
                // Конвертируем мировые координаты в UI координаты
                RectTransformUtility.ScreenPointToLocalPointInRectangle(  
                    _mainObjects.ToolRectTransform,
                    camera.WorldToScreenPoint(snappedWorldPosition),
                    camera,
                    out var localPoint
                );
                _rectTransformPositionTool.anchoredPosition = localPoint;
            };
            
            _transformComponent.XPosition.OnValueChanged += _toolFollowingObject;
            _transformComponent.YPosition.OnValueChanged += _toolFollowingObject;
        }
    }
}