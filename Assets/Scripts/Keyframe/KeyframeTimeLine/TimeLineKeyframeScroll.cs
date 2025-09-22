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

        public float Pan { get; private set; }
        
        [Inject]
        private void Construct(GameEventBus eventBus, MainObjects mainObjects)
        {
            _eventBus = eventBus;
            _mainObjects = mainObjects;
        }

        private void Awake()
        {
            _eventBus.SubscribeTo<MouseScrollDeltaY>(Calculate);
        }
        

        private void Calculate(ref MouseScrollDeltaY mouseScrollDeltaY)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    targetObject, 
                    UnityEngine.Input.mousePosition, 
                    targetCamera))
            {
                if (!UnityEngine.Input.GetKey(KeyCode.LeftControl) && !UnityEngine.Input.GetKey(KeyCode.LeftAlt) && !UnityEngine.Input.GetKey(KeyCode.LeftShift))
                {
                    _eventBus.Raise(new ScrollTimeLineKeyframeEvent(UnityEngine.Input.mouseScrollDelta.y * scrollMultiplier));
                }

            }
        }

        private void Update()
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    targetObject,
                    UnityEngine.Input.mousePosition,
                    targetCamera))
            {
                if(UnityEngine.Input.GetKey(KeyCode.LeftControl) && UnityEngine.Input.mouseScrollDelta.y != 0)
                {
                    _eventBus.Raise(new EventBus.Events.KeyframeTimeLine.OldPanEvent(Pan));
                    Pan += UnityEngine.Input.mouseScrollDelta.y * panMultiplier;
                    Pan = Mathf.Max(panMin, Pan);
                    _eventBus.Raise(new EventBus.Events.KeyframeTimeLine.PanEvent(Pan));
                }
            }
        }
    }
}