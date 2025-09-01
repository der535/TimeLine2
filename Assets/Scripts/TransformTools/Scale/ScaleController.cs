using System;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class ScaleController : MonoBehaviour
    {
        [FormerlySerializedAs("_gridScene")] [SerializeField] private GridScene gridScene;
        [SerializeField] private Camera camera;
        [SerializeField] private RectTransform tool;
        [SerializeField] private ScaleTool _scaleTool;

        private GameEventBus _eventBus;
        private MainObjects _mainObjects;

        private TransformComponent _transformComponent;

        private Action _objectFollowingTool;
        private Action _toolFollowingObject;

        private Action<float> _horizontalScale;
        private Action<float> _verticalScale;

        private float _startXScale;
        private float _startYScale;

        private Vector2 _initialToolSize;

        [Inject]
        private void Construct(GameEventBus gameEventBus, MainObjects mainObjects)
        {
            _eventBus = gameEventBus;
            _mainObjects = mainObjects;
        }

        private void Awake()
        {
            _eventBus.SubscribeTo(((ref SelectObjectEvent data) => SetPosition(data.Track.sceneObject)));

            _toolFollowingObject = () =>
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(  
                    _mainObjects.ToolRectTransform,
                    camera.WorldToScreenPoint(new Vector2(_transformComponent.XPosition.Value, _transformComponent.YPosition.Value)),
                    camera,
                    out var localPoint
                );
                tool.anchoredPosition = localPoint;
                
                // Обновляем размер инструмента при изменении позиции объекта
                UpdateToolSize();
            };

            _scaleTool.HorizontalDeltaStart += SetStartScale;
            _scaleTool.VerticalDeltaStart += SetStartScale;

            _scaleTool.HorizontalDelta += f => 
            {
                float newScale = gridScene.SnapToGrid(_startXScale * f);
                _transformComponent.XScale.Value = newScale;
                UpdateToolSize(); // Немедленно обновляем размер инструмента
            };
            
            _scaleTool.VerticalDelta += f => 
            {
                float newScale = gridScene.SnapToGrid(_startYScale * f);
                _transformComponent.YScale.Value = newScale;
                UpdateToolSize(); // Немедленно обновляем размер инструмента
            };
            
            // Сохраняем начальный размер инструмента
            _initialToolSize = tool.sizeDelta;
        }

        void SetStartScale()
        {
            if (_transformComponent)
            {
                _startXScale = gridScene.SnapToGrid(_transformComponent.XScale.Value);
                _startYScale = gridScene.SnapToGrid(_transformComponent.YScale.Value);
            }
        }

        private void UpdateToolSize()
        {
            if (_transformComponent == null) return;
            
            // Привязываем размер инструмента к сетке
            float snappedXScale = gridScene.SnapToGrid(_transformComponent.XScale.Value);
            float snappedYScale = gridScene.SnapToGrid(_transformComponent.YScale.Value);
            
            // Масштабируем инструмент пропорционально объекту
            tool.sizeDelta = new Vector2(
                _initialToolSize.x * snappedXScale,
                _initialToolSize.y * snappedYScale
            );
        }

        private void SetPosition(GameObject data)
        {
            print("SetPosition");
            if (_transformComponent)
            {
                _transformComponent.XPosition.OnValueChanged -= _toolFollowingObject;
                _transformComponent.YPosition.OnValueChanged -= _toolFollowingObject;
                _transformComponent.XScale.OnValueChanged -= UpdateToolSize;
                _transformComponent.YScale.OnValueChanged -= UpdateToolSize;
            }
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(  
                _mainObjects.ToolRectTransform,
                camera.WorldToScreenPoint(data.transform.position),
                camera,
                out var localPoint
            );
            
            tool.anchoredPosition = localPoint;

            _transformComponent = data.GetComponent<TransformComponent>();

            // Привязываем начальный масштаб к сетке
            _transformComponent.XScale.Value = gridScene.SnapToGrid(_transformComponent.XScale.Value);
            _transformComponent.YScale.Value = gridScene.SnapToGrid(_transformComponent.YScale.Value);
            
            tool.rotation = Quaternion.Euler(tool.rotation.x, tool.rotation.y, _transformComponent.ZRotation.Value);

            // Обновляем размер инструмента
            UpdateToolSize();

            _transformComponent.XPosition.OnValueChanged += _toolFollowingObject;
            _transformComponent.YPosition.OnValueChanged += _toolFollowingObject;
            _transformComponent.XScale.OnValueChanged += UpdateToolSize;
            _transformComponent.YScale.OnValueChanged += UpdateToolSize;
        }
    }
}