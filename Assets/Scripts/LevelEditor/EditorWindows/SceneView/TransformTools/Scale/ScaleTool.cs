using System;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;

namespace TimeLine
{
    public class ScaleTool : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [Header("References")]
        [SerializeField] private RectTransform _toolTransform;
        [SerializeField] private RectTransform _yHandle;
        [SerializeField] private RectTransform _yHandleCube;
        [SerializeField] private RectTransform _xHandle;
        [SerializeField] private RectTransform _xHandleCube;

        private enum ScaleMode { None, X, Y, All }
        private ScaleMode _currentScaleMode = ScaleMode.None;

        private float _initialYSize;
        private float _initialXSize;
        private Vector2 _dragStartPosition;

        public Action<float> VerticalDelta;
        public Action<float> HorizontalDelta;
        
        public Action VerticalDeltaStart;
        public Action HorizontalDeltaStart;
        
        public Action OnValueChanged;
        public Action OnStopX;
        public Action OnStopY;

        private void Awake()
        {
            CacheInitialSizes();
            VerticalDelta += (_) => OnValueChanged?.Invoke();
            HorizontalDelta += (_) => OnValueChanged?.Invoke();
        }

        private void CacheInitialSizes()
        {
            _initialYSize = _yHandle.sizeDelta.y;
            _initialXSize = _xHandle.sizeDelta.x;
        }

        public void StartScalingY()
        {
            VerticalDeltaStart.Invoke();
            StartScaling(ScaleMode.Y);
        }

        public void StartScalingX()
        {
            HorizontalDeltaStart.Invoke();
            StartScaling(ScaleMode.X);
        }

        public void StartScalingAll()
        {
            VerticalDeltaStart.Invoke();

            HorizontalDeltaStart.Invoke();

            StartScaling(ScaleMode.All);
        }

        public void StopScaling()
        {
            if (_currentScaleMode == ScaleMode.Y) ResetY();
            if (_currentScaleMode == ScaleMode.X) ResetX();
            if (_currentScaleMode == ScaleMode.All) ResetAll();
            _currentScaleMode = ScaleMode.None;
        }

        private void StartScaling(ScaleMode mode)
        {
            _currentScaleMode = mode;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _toolTransform,
                Mouse.current.position.ReadValue(),
                _camera,
                out var currentLocalPosition);
            _dragStartPosition = currentLocalPosition;
        }

        private void Update()
        {
            if (_currentScaleMode == ScaleMode.None) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _toolTransform,
                Mouse.current.position.ReadValue(),
                _camera,
                out var currentLocalPosition);
            
            var delta = _dragStartPosition - currentLocalPosition;

            switch (_currentScaleMode)
            {
                case ScaleMode.Y:
                    HandleYScaling(delta.y);
                    break;
                case ScaleMode.X:
                    HandleXScaling(delta.x);
                    break;
                case ScaleMode.All:
                    HandleAllScaling(-((delta.x + delta.y) / 2f));
                    break;
                case ScaleMode.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleYScaling(float verticalDelta)
        {
            float newSize = Mathf.Abs(_initialYSize - verticalDelta);
            bool isPositive = _initialYSize - verticalDelta > 0;
            
            VerticalDelta?.Invoke(-(verticalDelta/100-1));
            
            _yHandle.sizeDelta = new Vector2(_yHandle.sizeDelta.x, newSize);
            UpdateHandleAnchorAndPosition(_yHandle, _yHandleCube, newSize, isPositive, true);
        }

        private void HandleXScaling(float horizontalDelta)
        {
            float newSize = Mathf.Abs(_initialXSize - horizontalDelta);
            bool isPositive = _initialXSize - horizontalDelta > 0;
            
            HorizontalDelta?.Invoke(-(horizontalDelta/100-1));
            
            _xHandle.sizeDelta = new Vector2(newSize, _xHandle.sizeDelta.y);
            UpdateHandleAnchorAndPosition(_xHandle, _xHandleCube, newSize, isPositive, false);
        }

        private void HandleAllScaling(float combinedDelta)
        {
            float newYSize = Mathf.Abs(_initialYSize + combinedDelta);
            float newXSize = Mathf.Abs(_initialXSize + combinedDelta);
            
            bool yPositive = _initialYSize + combinedDelta > 0;
            bool xPositive = _initialXSize + combinedDelta > 0;
            
            _yHandle.sizeDelta = new Vector2(_yHandle.sizeDelta.x, newYSize);
            _xHandle.sizeDelta = new Vector2(newXSize, _xHandle.sizeDelta.y);
            
            HorizontalDelta?.Invoke(newYSize/100);
            VerticalDelta?.Invoke(newXSize/100);
            
            UpdateHandleAnchorAndPosition(_yHandle, _yHandleCube, newYSize, yPositive, true);
            UpdateHandleAnchorAndPosition(_xHandle, _xHandleCube, newXSize, xPositive, false);
        }

        private void UpdateHandleAnchorAndPosition(
            RectTransform handle, 
            RectTransform handleCube,
            float newSize,
            bool isPositive,
            bool isVertical)
        {
            Vector2 anchor = isPositive ? 
                (isVertical ? new Vector2(0.5f, 1f) : new Vector2(1f, 0.5f)) : 
                (isVertical ? new Vector2(0.5f, 0f) : new Vector2(0f, 0.5f));
            
            handleCube.anchorMin = anchor;
            handleCube.anchorMax = anchor;
            handleCube.anchoredPosition = Vector2.zero;
            
            float positionOffset = isPositive ? newSize / 2 : -newSize / 2;
            handle.anchoredPosition = isVertical ? 
                new Vector2(handle.anchoredPosition.x, positionOffset) : 
                new Vector2(positionOffset, handle.anchoredPosition.y);
        }
        
        
        private void ResetY()
        {
            _yHandle.sizeDelta = new Vector2(_yHandle.sizeDelta.x, _initialYSize);
            _yHandle.anchoredPosition = new Vector2(_yHandle.anchoredPosition.x, _initialYSize / 2);
            SetDefaultAnchor(_yHandleCube, true);
            OnStopY.Invoke();
        }

        private void ResetX()
        {
            _xHandle.sizeDelta = new Vector2(_initialXSize, _xHandle.sizeDelta.y);
            _xHandle.anchoredPosition = new Vector2(_initialXSize / 2, _xHandle.anchoredPosition.y);
            SetDefaultAnchor(_xHandleCube, false);
            OnStopX.Invoke();
        }

        private void ResetAll()
        {
            ResetY();
            ResetX();
        }

        private void SetDefaultAnchor(RectTransform handleCube, bool isVertical)
        {
            Vector2 anchor = isVertical ? new Vector2(0.5f, 1f) : new Vector2(1f, 0.5f);
            handleCube.anchorMin = anchor;
            handleCube.anchorMax = anchor;
            handleCube.anchoredPosition = Vector2.zero;
        }
    }
}