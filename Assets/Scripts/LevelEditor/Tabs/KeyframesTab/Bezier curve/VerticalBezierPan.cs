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
        private ActionMap _actionMap;
        
        public float Pan => _pan;
        public float OldPan => _oldPan;
        
        private GameEventBus _eventBus;
        

        [Inject]
        private void Construct(GameEventBus gameEventBus, ActionMap actionMap)
        {
            _eventBus = gameEventBus;
            _actionMap = actionMap;
        }

        private void Start()
        {
            _actionMap.Editor.MouseScroll.started += context =>
            {
                Calculate((int)context.ReadValue<float>());
            };
        }

        internal void SetPan(float newPan)
        {
            _oldPan = _pan;
            _pan = newPan;
            _eventBus.Raise(new PanBezier(Pan, OldPan));
        }

        private void Calculate(int delta)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    _rightPanel, 
                    UnityEngine.Input.mousePosition, 
                    _mainCamera))
            {
                if (_actionMap.Editor.LeftShift.IsPressed())
                {
                    _oldPan = _pan;
                    _pan += delta * PanMultiplier;
                    _eventBus.Raise(new PanBezier(Pan, OldPan));
                }
            }
        }
    }
}
