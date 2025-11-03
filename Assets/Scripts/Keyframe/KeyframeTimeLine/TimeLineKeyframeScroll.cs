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

        public float Pan { get; private set; }
        
        [Inject]
        private void Construct(GameEventBus eventBus, MainObjects mainObjects, ActionMap actionMap)
        {
            _eventBus = eventBus;
            _mainObjects = mainObjects;
            _actionMap = actionMap;
        }

        private void Awake()
        {
            _eventBus.SubscribeTo<MouseScrollDeltaY>(Calculate);
            

            _actionMap.Editor.MouseScroll.started += _ =>
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(
                        targetObject,
                        UnityEngine.Input.mousePosition,
                        targetCamera)) return;
                
                if(!_actionMap.Editor.LeftCtrl.IsPressed()) return;
                
                var mouseScroll = _actionMap.Editor.MouseScroll.ReadValue<float>();
                
                _eventBus.Raise(new EventBus.Events.KeyframeTimeLine.OldPanEvent(Pan));
                Pan += mouseScroll * panMultiplier;
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

        private void Calculate(ref MouseScrollDeltaY mouseScrollDeltaY)
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