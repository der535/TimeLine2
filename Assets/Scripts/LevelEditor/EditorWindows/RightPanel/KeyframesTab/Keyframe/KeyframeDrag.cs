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
        private TimeLineKeyframeScroll _timeLineKeyframeScroll;
        private KeyfeameVizualizer _keyframeVizualizer;

        private Vector2 _startMousePosition;
        private Vector2 _startObjectPosition;

        public Keyframe _keyframe { get; private set; }

        private Action _sortKeyframes;

        private bool _isDragging;

        [Inject]
        private void Construct(
            MainObjects mainObject, 
            TimeLineConverter timeLineConverter, 
            GridUI gridUI,
            TimeLineKeyframeScroll timeLineKeyframeScroll,
            KeyfeameVizualizer keyframeVizualizer)
        {
            _gridUI = gridUI;
            _mainObjects = mainObject;
            _timeLineConverter = timeLineConverter;
            _timeLineKeyframeScroll = timeLineKeyframeScroll;
            _keyframeVizualizer = keyframeVizualizer;
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
                // Вычисляем новую позицию без учета смещения корня
                float newPositionX = _startObjectPosition.x - (_startMousePosition.x - GetMousePosition().x);
                
                // Применяем округление к позиции относительно корня
                
                float rootOffset = _mainObjects.KeyframeRootRectTransform.offsetMin.x - _mainObjects.KeyframeScrollView.offsetMin.x - _mainObjects.KeyframeVerticalLayoutGroup.padding.left / 2f;
                
                // float rootOffset = _mainObjects.KeyframeRootRectTransform.offsetMin.x;
                float relativePosition = newPositionX - rootOffset;
                float roundedRelativePosition = _gridUI.RoundAnchorPositionToGrid(relativePosition, _timeLineKeyframeScroll.Zoom);
                float finalPositionX = roundedRelativePosition + rootOffset;

                _rectTransform.anchoredPosition = new Vector2(finalPositionX, _rectTransform.anchoredPosition.y);

                // Вычисляем тики на основе относительной позиции
                var startTick = _keyframe.Ticks;
                _keyframe.Ticks = MathF.Round((float)_timeLineConverter.SecondsToTicks(
                    _timeLineConverter.GetTimeFromAnchorPosition(roundedRelativePosition, _timeLineKeyframeScroll.Zoom)));
                _keyframeVizualizer.MultipleDrag(_keyframe.Ticks - startTick, this);
                    
                _sortKeyframes.Invoke();
            }
        }
    }
}