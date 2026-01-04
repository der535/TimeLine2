using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class VerticalBezierZoom : MonoBehaviour
    {
        [SerializeField] private RectTransform _rightPanel;
        [SerializeField] private Camera _mainCamera;

        [Space]
        [SerializeField] private float _minZoom = 0.1f;
        [SerializeField] private float _maxZoom = 1000f;
        // Коэффициент чувствительности зума (обычно от 0.05 до 0.2)
        private const float ZoomSensitivity = 0.1f;
        
        private float _zoom = 70;
        private float _oldZoom = 0;
        private ActionMap _actionMap;
        
        public float Zoom => _zoom;
        public float OldZoom => _oldZoom;
        
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
                // Получаем направление скролла: 1 или -1
                float scrollDelta = context.ReadValue<float>();
                Calculate(Mathf.Sign(scrollDelta));
            };
        }

        internal void SetPan(float newPan)
        {
            _oldZoom = _zoom;
            _zoom = newPan;
            _eventBus.Raise(new ZoomBezier(Zoom, OldZoom));
        }

        private void Calculate(float direction)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    _rightPanel, 
                    UnityEngine.Input.mousePosition, 
                    _mainCamera))
            {
                if (_actionMap.Editor.LeftShift.IsPressed())
                {
                    _oldZoom = _zoom;

                    // Экспоненциальная формула: 
                    // Новое значение = Текущее * (Основание ^ Направление)
                    // Это гарантирует, что изменение всегда пропорционально текущему масштабу.
                    float zoomFactor = Mathf.Pow(1f + ZoomSensitivity, direction);
                    _zoom *= zoomFactor;

                    // Опционально: ограничиваем значения, чтобы зум не ушел в бесконечность или 0
                    _zoom = Mathf.Clamp(_zoom, _minZoom, _maxZoom);

                    _eventBus.Raise(new ZoomBezier(Zoom, OldZoom));
                }
            }
        }
    }
}