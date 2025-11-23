
using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.Installers;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class TimeLineScroll : MonoBehaviour
    {
        [SerializeField] private RectTransform timeLineRect;
        [Space]
        [SerializeField] private float scrollMultiplier;
        [SerializeField] private float panMultiplier;
        [SerializeField] private float horizontalScroll;
        [Space]
        [SerializeField] private float panMin;
        [SerializeField] private float panMax;
        [SerializeField] private float panFactor;
        [Space]
        [SerializeField] private RectTransform targetObject;
        [SerializeField] private TimeLineSettings timeLineSettings;

        private GameEventBus _eventBus;
        private MainObjects _mainObjects;
        private ActionMap _actionMap;

        public float Pan { get; private set; }
        
        [Inject]
        private void Construct(GameEventBus eventBus, MainObjects mainObjects, ActionMap actionMap)
        {
            _eventBus = eventBus;
            _mainObjects = mainObjects;
            _actionMap = actionMap;
            Pan = 1;
        }
        
        private bool GetCursorPosition()
        {
            return RectTransformUtility.RectangleContainsScreenPoint(timeLineRect,
                UnityEngine.Input.mousePosition, _mainObjects.MainCamera);
        }

        private void Awake()
        {
            _eventBus.SubscribeTo<MouseScrollDeltaY>(Calculate);
            
        }
        
        
        private void Calculate(ref MouseScrollDeltaY mouseScrollDeltaY)
        {
            if (!GetCursorPosition()) return;

            var mouseScrollDelta = _actionMap.Editor.MouseScroll.ReadValue<float>();
            
            if (mouseScrollDelta > targetObject.sizeDelta.y) return;

            float scroll = mouseScrollDelta;

            if (!_actionMap.Editor.LeftAlt.IsPressed())
            {
                if(!_actionMap.Editor.LeftCtrl.IsPressed())
                    _eventBus.Raise(new ScrollTimeLineEvent(scroll * scrollMultiplier));
            }
            else
            {
                _eventBus.Raise(new OldPanEvent(Pan));

                float currentSpacing = timeLineSettings.DistanceBetweenBeatLines + Pan;
                // Используем panFactor как базу экспоненты
                float factor = Mathf.Pow(panFactor, scroll);
                float newSpacing = currentSpacing * factor;

                newSpacing = Mathf.Clamp(newSpacing, panMin, panMax);
                Pan = newSpacing - timeLineSettings.DistanceBetweenBeatLines;

                _eventBus.Raise(new PanEvent(Pan));
            }
        }
    }
}
