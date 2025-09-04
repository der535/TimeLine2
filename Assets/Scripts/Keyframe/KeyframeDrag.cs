using System;
using TimeLine.Installers;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace TimeLine.Keyframe
{
    public class KeyframeDrag : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private MainObjects _mainObjects;
        private TimeLineConverter _timeLineConverter;
        private GridUI _gridUI;

        private Vector2 _startMousePosition;
        private Vector2 _startObjectPosition;

        public Keyframe _keyframe { get; private set; }

        private Action _sortKeyframes;

        private bool _isDragging;
        
        [Inject]
        private void Construct(MainObjects mainObject, TimeLineConverter timeLineConverter, GridUI gridUI)
        {
            _gridUI = gridUI;
            _mainObjects = mainObject;
            _timeLineConverter = timeLineConverter;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Setup(Keyframe keyframe, Action sortKeyframes)
        {
            _keyframe = keyframe;
            _sortKeyframes = sortKeyframes;
        }

        private Vector2 GetMousePosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle
            (_mainObjects.CanvasRectTransform, Mouse.current.position.ReadValue(), _mainObjects.MainCamera,
                out var localPoint);
            return localPoint;
        }

        public void Drag(bool isDragging)
        {
            _isDragging = isDragging;

            if (!isDragging) return;
            _startMousePosition = GetMousePosition();
            _startObjectPosition = _rectTransform.anchoredPosition;
        }

        private void Update()
        {
            if (_isDragging)
            {
                _rectTransform.anchoredPosition =
                    new Vector2(
                        _gridUI.RoundAnchorPositionToGrid(_startObjectPosition.x -
                                                          (_startMousePosition.x - GetMousePosition().x)),
                        _rectTransform.anchoredPosition.y);

                _keyframe.ticks = MathF.Round((float)_timeLineConverter.SecondsToTicks(
                        _timeLineConverter.GetTimeFromAnchorPosition(_rectTransform.anchoredPosition.x)));
                _sortKeyframes.Invoke();
            }
        }
    }
}