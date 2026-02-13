using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.Installers;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine
{
    /// <summary>
    /// Отвечает за зум в таймлайне ключевых кадров
    /// </summary>
    public class TimeLineKeyframeZoom : MonoBehaviour
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

        public float Zoom { get; private set; } = 70;
        
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
        
                _eventBus.Raise(new EventBus.Events.KeyframeTimeLine.KeyframeOldZoomEvent(Zoom));

                // --- Экспоненциальное изменение ---
                // Если mouseScroll > 0, зум увеличивается (умножаем на число > 1)
                // Если mouseScroll < 0, зум уменьшается (делим или умножаем на число < 1)
                float zoomFactor = Mathf.Pow(1.1f, mouseScroll); 
                Zoom *= zoomFactor;
                // ----------------------------------

                Zoom = Mathf.Max(panMin, Zoom);
                _eventBus.Raise(new KeyframeZoomEvent(Zoom));
            };
        }

        internal void SetZoom(float value)
        {
            _eventBus.Raise(new KeyframeOldZoomEvent(Zoom));
            Zoom = value;
            Zoom = Mathf.Max(panMin, Zoom);
            _eventBus.Raise(new KeyframeZoomEvent(Zoom));
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