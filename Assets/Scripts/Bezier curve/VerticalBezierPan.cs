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

        private const float PanMultiplier = 10;
        
        private float _pan = 70;
        private float _oldPan = 0;
        
        public float Pan => _pan;
        public float OldPan => _oldPan;
        
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

        internal void SetPan(float newPan)
        {
            _oldPan = _pan;
            _pan = newPan;
            _eventBus.Raise(new PanBezier(Pan, OldPan));
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
                    _oldPan = _pan;
                    _pan += mouseScrollDeltaY.Y * PanMultiplier;
                    _eventBus.Raise(new PanBezier(Pan, OldPan));
                }
            }
        }
    }
}
