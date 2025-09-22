using System;
using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class VerticalBezierScroll : MonoBehaviour
    {
        [SerializeField] private RectTransform _rightPanel;
        [SerializeField] private Camera _mainCamera;

        public const float ScrollMultiplier = 70;
        
        private float _verticalScroll;
        
        public float VerticalScroll => _verticalScroll;
        
        private GameEventBus _eventBus;
        

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _eventBus = gameEventBus;
        }

        private void Start()
        {
            _eventBus.SubscribeTo<MouseScrollDeltaY>(Calculate);
        }

        private void Calculate(ref MouseScrollDeltaY mouseScrollDeltaY)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    _rightPanel, 
                    UnityEngine.Input.mousePosition, 
                    _mainCamera))
            {
                if (UnityEngine.Input.GetKey(KeyCode.LeftAlt))
                {
                    _verticalScroll += mouseScrollDeltaY.Y;
                    _eventBus.Raise(new ScrollBezier(_verticalScroll * ScrollMultiplier));
                }
            }
        }
    }
}
