using System;
using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.Installers;
using UnityEngine;
using Zenject;


namespace TimeLine
{
    public class TimeLineKeyframeScroll : MonoBehaviour
    {
        [SerializeField] private float scrollMultiplier;
        [SerializeField] private float panMultiplier;
        [SerializeField] private float horizontalScroll;
        [Space]
        [SerializeField] private float panMin;
        [SerializeField] private float panFactor;
        [Space]
        [SerializeField] private RectTransform targetObject;
        [SerializeField] private Camera targetCamera;

        private GameEventBus _eventBus;
        private MainObjects _mainObjects;
        private ActionMap _actionMap;

        public float Pan { get; private set; } = 70;
        
        [Inject]
        private void Construct(GameEventBus eventBus, MainObjects mainObjects, ActionMap actionMap)
        {
            _eventBus = eventBus;
            _mainObjects = mainObjects;
            _actionMap = actionMap;
        }

        private void Awake()
        {
            _actionMap.Editor.MouseScroll.started += _ =>
            {
                Calculate();
        
                if (!RectTransformUtility.RectangleContainsScreenPoint(
                        targetObject,
                        UnityEngine.Input.mousePosition,
                        targetCamera)) return;
        
                if(!_actionMap.Editor.LeftAlt.IsPressed()) return;
        
                var mouseScroll = _actionMap.Editor.MouseScroll.ReadValue<float>();
        
                _eventBus.Raise(new EventBus.Events.KeyframeTimeLine.OldPanEvent(Pan));

                // --- Экспоненциальное изменение ---
                // Если mouseScroll > 0, зум увеличивается (умножаем на число > 1)
                // Если mouseScroll < 0, зум уменьшается (делим или умножаем на число < 1)
                float zoomFactor = Mathf.Pow(1.1f, mouseScroll); 
                Pan *= zoomFactor;
                // ----------------------------------

                Pan = Mathf.Max(panMin, Pan);
                _eventBus.Raise(new EventBus.Events.KeyframeTimeLine.PanEvent(Pan));
            };
        }

        internal void SetPan(float value)
        {
            _eventBus.Raise(new EventBus.Events.KeyframeTimeLine.OldPanEvent(Pan));
            Pan = value;
            Pan = Mathf.Max(panMin, Pan);
            _eventBus.Raise(new EventBus.Events.KeyframeTimeLine.PanEvent(Pan));
        }

        private void Calculate()
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    targetObject, 
                    UnityEngine.Input.mousePosition, 
                    targetCamera))
            {
                if(!_actionMap.Editor.LeftCtrl.IsPressed() && !_actionMap.Editor.LeftAlt.IsPressed() && !_actionMap.Editor.LeftShift.IsPressed())
                {
                    _eventBus.Raise(new ScrollTimeLineKeyframeEvent(UnityEngine.Input.mouseScrollDelta.y * scrollMultiplier));
                }
            }
        }
    }
}