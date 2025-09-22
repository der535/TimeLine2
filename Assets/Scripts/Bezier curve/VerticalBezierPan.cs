using System;
using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class VerticalBezierPan : MonoBehaviour
    {
        [SerializeField] private RectTransform _rightPanel;
        [SerializeField] private Camera _mainCamera;

        private const float PanMultiplier = 20;
        
        private float _pan;
        
        public float Pan => _pan * PanMultiplier;
        
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
                if (UnityEngine.Input.GetKey(KeyCode.LeftShift))
                {
                    _pan += mouseScrollDeltaY.Y;
                    _eventBus.Raise(new PanBezier(_pan * PanMultiplier));
                }
            }
        }
    }
}
