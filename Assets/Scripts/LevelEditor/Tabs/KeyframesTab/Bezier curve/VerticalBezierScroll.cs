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

        private const float ScrollMultiplier = 70;
        
        private float _verticalScroll;
        
        public float VerticalScroll => _verticalScroll;
        
        private GameEventBus _eventBus;
        private ActionMap _actionMap;
        

        [Inject]
        private void Construct(GameEventBus gameEventBus, ActionMap actionMap)
        {
            _eventBus = gameEventBus;
            _actionMap = actionMap;
        }

        private void Start()
        {
            _actionMap.Editor.MouseScroll.started += data =>
            {
                Calculate(data.ReadValue<float>());
            };
        }

        private void Calculate(float value)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    _rightPanel, 
                    UnityEngine.Input.mousePosition, 
                    _mainCamera))
            {
                if (_actionMap.Editor.LeftCtrl.IsPressed())
                {
                    _eventBus.Raise(new ScrollBezier(value * ScrollMultiplier));
                }
            }
        }
    }
}
