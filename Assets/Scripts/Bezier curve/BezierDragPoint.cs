using System;
using TimeLine.Installers;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace TimeLine
{
    public class BezierDragPoint : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        private MainObjects _mainObjects;
        private TimeLineConverter _timeLineConverter;
        private GridUI _gridUI;
        private TimeLineKeyframeScroll _timeLineKeyframeScroll;
        private VerticalBezierPan _verticalBezierPan;
        private BezierLineDrawer _bezierLineDrawer;

        private Vector2 _startMousePosition;
        private Vector2 _startObjectPosition;

        public Keyframe.Keyframe _keyframe { get; private set; }

        private Action _sortKeyframes;

        private bool _isDragging;

        [Inject]
        private void Construct(
            MainObjects mainObject, 
            TimeLineConverter timeLineConverter, 
            GridUI gridUI,
            TimeLineKeyframeScroll timeLineKeyframeScroll,
            VerticalBezierPan verticalBezierPan,
            BezierLineDrawer bezierLineDrawer)
        {
            _gridUI = gridUI;
            _mainObjects = mainObject;
            _timeLineConverter = timeLineConverter;
            _timeLineKeyframeScroll = timeLineKeyframeScroll;
            _verticalBezierPan = verticalBezierPan;
            _bezierLineDrawer = bezierLineDrawer;
        }
        
        public void Setup(Keyframe.Keyframe keyframe, Action sortKeyframes)
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
                #region Horizontal

                    // Вычисляем новую позицию без учета смещения корня
                    float newPositionX = _startObjectPosition.x - (_startMousePosition.x - GetMousePosition().x);
                    
                    // Применяем округление к позиции относительно корня
                    
                    float rootOffset = _mainObjects.KeyframeRootRectTransform.offsetMin.x - _mainObjects.KeyframeScrollView.offsetMin.x - _mainObjects.KeyframeVerticalLayoutGroup.padding.left / 2f;
                    
                    // float rootOffset = _mainObjects.KeyframeRootRectTransform.offsetMin.x;
                    float relativePosition = newPositionX - rootOffset;
                    float roundedRelativePosition = _gridUI.RoundAnchorPositionToGrid(relativePosition, _timeLineKeyframeScroll.Pan);
                    float finalPositionX = roundedRelativePosition + rootOffset;

                    _rectTransform.anchoredPosition = new Vector2(finalPositionX, _rectTransform.anchoredPosition.y);

                    // Вычисляем тики на основе относительной позиции
                    _keyframe.Ticks = MathF.Round((float)_timeLineConverter.SecondsToTicks(
                        _timeLineConverter.GetTimeFromAnchorPosition(roundedRelativePosition, _timeLineKeyframeScroll.Pan)));
                        
                    _sortKeyframes?.Invoke();

                #endregion

                #region Vertical
                    
                    // Вычисляем новую позицию без учета смещения корня
                    float newPositionY = _startObjectPosition.y - (_startMousePosition.y - GetMousePosition().y);
                    
                    _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, newPositionY);
                    
                    _keyframe.GetData().SetValue(_rectTransform.anchoredPosition.y / _verticalBezierPan.Pan);
                
                #endregion
                
                _bezierLineDrawer.UpdateBezierCurve();
            }
        }
    }
}