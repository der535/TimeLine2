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

        [Inject]
        private void Construct(GameEventBus gameEventBus, MainObjects mainObjects)
        {
            _eventBus = gameEventBus;
            _mainObjects = mainObjects;
        }

        private void Awake()
        {
            _eventBus.SubscribeTo<SelectSceneObject>(SetPosition);

            _toolFollowingObject = () =>
            {
                Vector2 position = camera.WorldToScreenPoint(new Vector2(_transformComponent.XPosition.Value,
                    _transformComponent.YPosition.Value));
                position -= _mainObjects.CanvasRectTransform.sizeDelta / 2;
                tool.anchoredPosition = position;
            };

            _scaleTool.HorizontalDeltaStart += SetStartScale;
            _scaleTool.VerticalDeltaStart += SetStartScale;

            _scaleTool.HorizontalDelta += f => _transformComponent.XScale.Value = gridScene.ScaleSnapToGrid(_startXScale * f);
            _scaleTool.VerticalDelta += f => _transformComponent.YScale.Value = gridScene.ScaleSnapToGrid(_startYScale * f);
        }

        void SetStartScale()
        {
            if (_transformComponent)
            {
                _startXScale = _transformComponent.XScale.Value;
                _startYScale = _transformComponent.YScale.Value;
            }
        }

        private void SetPosition(ref SelectSceneObject data)
        {
            if (_transformComponent)
            {
                _transformComponent.XPosition.OnValueChanged -= _toolFollowingObject;
                _transformComponent.YPosition.OnValueChanged -= _toolFollowingObject;
            }
            
            tool.anchoredPosition = camera.WorldToScreenPoint(data.GameObject.transform.position);
            tool.anchoredPosition -= _mainObjects.CanvasRectTransform.sizeDelta / 2;

            _transformComponent = data.GameObject.GetComponent<TransformComponent>();

            tool.rotation = Quaternion.Euler(tool.rotation.x, tool.rotation.y, _transformComponent.ZRotation.Value);

            _transformComponent.XPosition.OnValueChanged += _toolFollowingObject;
            _transformComponent.YPosition.OnValueChanged += _toolFollowingObject;
        }
    }
}