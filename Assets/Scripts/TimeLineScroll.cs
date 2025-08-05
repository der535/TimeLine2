
using System;
using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.Installers;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class TimeLineScroll : MonoBehaviour
    {
        [SerializeField] private float scrollMultiplier;
        [SerializeField] private float panMultiplier;
        [SerializeField] private float horizontalScroll;
        [Space]
        [SerializeField] private float panMin;
        [SerializeField] private float panFactor;
        [Space]
        [SerializeField] private RectTransform targetObject;

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
            if(UnityEngine.Input.mousePosition.y > targetObject.sizeDelta.y) return;
            
            if (!UnityEngine.Input.GetKey(KeyCode.LeftControl))
            {
                _eventBus.Raise(new ScrollTimeLineEvent(UnityEngine.Input.mouseScrollDelta.y * scrollMultiplier));
            }
            else if(UnityEngine.Input.GetKey(KeyCode.LeftControl))
            {
                _eventBus.Raise(new OldPanEvent(Pan));
                Pan += UnityEngine.Input.mouseScrollDelta.y * panMultiplier;
                Pan = Mathf.Max(panMin, Pan);
                _eventBus.Raise(new PanEvent(Pan));
                print(Pan);
            }
        }
    }
}
