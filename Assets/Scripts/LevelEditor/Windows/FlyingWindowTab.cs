using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TimeLine
{
    public class FlyingWindowTab : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _text;

        private RectTransform _rectTransform;
        private Camera _mainCamera;
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;

        public float maxTiltAngle = 30f;
        public float tiltSpeed = 10f;

        private Vector3 _targetPosition;
        private Vector3 _lastPosition;
        private float _currentTilt;

        private bool _move = false;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _mainCamera = Camera.main;
            _canvas = GetComponentInParent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            if (!_move)
                return;

            Mouse mouse = Mouse.current;
            if (mouse == null) return;
            Vector3 mousePosition = mouse.position.ReadValue();

            // Конвертируем экранные координаты в координаты Canvas
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                mousePosition.z = _canvas.planeDistance;
                _targetPosition = _mainCamera.ScreenToWorldPoint(mousePosition);
            }
            else if (_canvas.renderMode == RenderMode.WorldSpace)
            {
                Ray ray = _mainCamera.ScreenPointToRay(mousePosition);
                Plane plane = new Plane(Vector3.forward, _rectTransform.position.z);
                float distance;
                if (plane.Raycast(ray, out distance))
                {
                    _targetPosition = ray.GetPoint(distance);
                }
            }
            else // Screen Space Overlay
            {
                _targetPosition = mousePosition;
            }

            Vector3 oldPosition = _rectTransform.position;
            _rectTransform.position = _targetPosition;

            float deltaX = _rectTransform.position.x - oldPosition.x;
            float targetTilt = Mathf.Clamp(deltaX * 200f, -maxTiltAngle, maxTiltAngle);
            _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
            _rectTransform.localRotation = Quaternion.Euler(0, 0, _currentTilt);
        }

        public void StartMove(string name, Sprite sprite)
        {
            _move = true;

            _image.sprite = sprite;
            _text.text = name;

            float textWidth = _text.GetPreferredValues(name).x;

            var size = _rectTransform.sizeDelta;
            size.x = textWidth;
            _rectTransform.sizeDelta = size;

            LMotion.Create(Vector3.one, new Vector3(1.1f, 1.1f, 1.1f), 0.5f).WithEase(Ease.OutBounce).BindToLocalScale(_rectTransform);
            LMotion.Create(0f, 1f, 0.2f).BindToAlpha(_canvasGroup);
        }

        public void StopMove()
        {
            _move = false;
            LMotion.Create(new Vector3(1.1f, 1.1f, 1.1f), Vector3.one, 0.2f).WithEase(Ease.InExpo).BindToLocalScale(_rectTransform);
            LMotion.Create(1f, 0f, 0.1f).BindToAlpha(_canvasGroup);
        }
    }
}
