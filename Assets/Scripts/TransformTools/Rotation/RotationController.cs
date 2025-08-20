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

        private Action _toolFollowingObject;

        private GameEventBus _gameEventBus;
        private TransformComponent _transformComponent;
        private MainObjects _mainObjects;

        [Inject]
        private void Construct(GameEventBus gameEventBus, MainObjects mainObjects)
        {
            _gameEventBus = gameEventBus;
            _mainObjects = mainObjects;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo<SelectSceneObject>(Select);
            rotateTool.onRotate = (value) => _transformComponent.ZRotation.Value = value;
        }

        private void Select(ref SelectSceneObject data)
        {
            _transformComponent = data.GameObject.GetComponent<TransformComponent>();
            rotateTool.currentRotation = _transformComponent.ZRotation.Value;
            
            bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                toolCanvas, // RectTransform, в системе координат которого нужно получить точку
                camera.WorldToScreenPoint(new Vector2(_transformComponent.XPosition.Value,
                    _transformComponent.YPosition.Value)) , // точка в экранных координатах
                camera, // для Overlay-канваса передаём null, для World Space — камеру
                out var localPoint // результат: точка в локальных координатах RectTransform
            );
            
            tool.anchoredPosition = localPoint;
        }
    }
}