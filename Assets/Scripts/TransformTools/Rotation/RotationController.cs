using System;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class RotationController : MonoBehaviour
    {
        [SerializeField] private RectTransform tool;
        [SerializeField] private RotateTool rotateTool;
        [SerializeField] private Camera camera;
        [SerializeField] private RectTransform toolCanvas;
        [SerializeField] private GridScene gridScene;

        private Action _toolFollowingObject;

        private GameEventBus _gameEventBus;
        private TransformComponent _transformComponent;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo(((ref SelectObjectEvent data) => Select(data.Track.sceneObject)));
            
            rotateTool.onRotate = (value) => _transformComponent.ZRotation.Value = gridScene.RotateSnapToGrid(value);

            _toolFollowingObject += (() =>
            {
                bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    toolCanvas, // RectTransform, в системе координат которого нужно получить точку
                    camera.WorldToScreenPoint(new Vector2(_transformComponent.XPosition.Value,
                        _transformComponent.YPosition.Value)), // точка в экранных координатах
                    camera, // для Overlay-канваса передаём null, для World Space — камеру
                    out var localPoint // результат: точка в локальных координатах RectTransform
                );

                tool.anchoredPosition = localPoint;
            });
        }

        private void Select(GameObject data)
        {
            if (_transformComponent)
            {
                _transformComponent.XPosition.OnValueChanged -= _toolFollowingObject;
                _transformComponent.YPosition.OnValueChanged -= _toolFollowingObject;
            }

            
            _transformComponent = data.GetComponent<TransformComponent>();
            rotateTool.currentRotation = _transformComponent.ZRotation.Value;
            
            bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                toolCanvas, // RectTransform, в системе координат которого нужно получить точку
                camera.WorldToScreenPoint(new Vector2(_transformComponent.XPosition.Value,
                    _transformComponent.YPosition.Value)) , // точка в экранных координатах
                camera, // для Overlay-канваса передаём null, для World Space — камеру
                out var localPoint // результат: точка в локальных координатах RectTransform
            );
            
            tool.anchoredPosition = localPoint;
            
            _transformComponent.XPosition.OnValueChanged += _toolFollowingObject;
            _transformComponent.YPosition.OnValueChanged += _toolFollowingObject;
        }
    }
}