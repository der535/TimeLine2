using System;
using TimeLine.Installers;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class BezierDragPoint : MonoBehaviour
    {
        [SerializeField] private BezierPoint bezierPoint;
        [SerializeField] private BezierPointTangleLineDrawer bezierPointTangleLineDrawer;
        [Space]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RectTransform tangentLeft;
        [SerializeField] private RectTransform tangentRight;
        
        private TimeLineKeyframeScroll _timeLineKeyframeScroll;
        private VerticalBezierPan _verticalBezierPan;
        private TimeLineConverter _timeLineConverter;
        private BezierLineDrawer _bezierLineDrawer;
        private MainObjects _mainObjects;
        private BezierController _bezierController;
        private TimeLineSettings _timeLineSettings;
        private Main _main;
        
        private Vector2 _startMousePosition;
        private Vector2 _startObjectPosition;
        private Action _sortKeyframes;
        
        private GridUI _gridUI;
        private bool _isDragging;
        private bool _isDraggingTangleLeft;
        private bool _isDraggingTangleRight;

        public Keyframe.Keyframe _keyframe { get; private set; }
        
        [Inject]
        private void Construct(
            MainObjects mainObject, 
            TimeLineConverter timeLineConverter, 
            GridUI gridUI,
            TimeLineKeyframeScroll timeLineKeyframeScroll,
            VerticalBezierPan verticalBezierPan,
            BezierLineDrawer bezierLineDrawer,
            BezierController bezierController,
            TimeLineSettings timeLineSettings,
            Main main)
        {
            _gridUI = gridUI;
            _mainObjects = mainObject;
            _timeLineConverter = timeLineConverter;
            _timeLineKeyframeScroll = timeLineKeyframeScroll;
            _verticalBezierPan = verticalBezierPan;
            _bezierLineDrawer = bezierLineDrawer;
            _bezierController = bezierController;
            _timeLineSettings = timeLineSettings;
            _main = main;
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
        
        private Vector2 GetMousePosition(RectTransform root)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle
            (root, Mouse.current.position.ReadValue(), _mainObjects.MainCamera,
                out var localPoint);
            return localPoint;
        }

        public void Drag(bool isDragging)
        {
            _isDragging = isDragging;

            if (!isDragging) return;
            _startMousePosition = GetMousePosition();
            _startObjectPosition = rectTransform.anchoredPosition;
        }
        
        public void DragTangentLeftPoint(bool drag)
        {
            _isDraggingTangleLeft = drag;
        }

        public void DragTangentRightPoint(bool drag)
        {
            _isDraggingTangleRight = drag;
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

                    print(rectTransform);
                    
                    rectTransform.anchoredPosition = new Vector2(finalPositionX, rectTransform.anchoredPosition.y);

                    // Вычисляем тики на основе относительной позиции
                    _keyframe.Ticks = MathF.Round((float)_timeLineConverter.SecondsToTicks(
                        _timeLineConverter.GetTimeFromAnchorPosition(roundedRelativePosition, _timeLineKeyframeScroll.Pan)));
                        
                    _sortKeyframes?.Invoke();

                #endregion

                #region Vertical
                    
                    // Вычисляем новую позицию без учета смещения корня
                    float newPositionY = _startObjectPosition.y - (_startMousePosition.y - GetMousePosition().y);
                    
                    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newPositionY);
                    
                    _keyframe.GetData().SetValue(rectTransform.anchoredPosition.y / _verticalBezierPan.Pan);
                
                #endregion

                _bezierLineDrawer.SortPoints();
                print(_bezierController);
                _bezierController.SortPoints();
                _bezierController.UpdatePositions();
                _bezierLineDrawer.UpdateBezierCurve();
                bezierPointTangleLineDrawer.UpdatePosition();
            }

            if (_isDraggingTangleRight)
            {

                
                // Вычисляем новую позицию без учета смещения корня
                Vector2 position = tangentRight.anchoredPosition - (tangentRight.anchoredPosition - GetMousePosition(rectTransform));
                
                if(position.x < 0.1f) 
                    position = new Vector2(0.1f,position.y);
                
                tangentRight.anchoredPosition = position;

                float tangleValue = tangentRight.anchoredPosition.y / _verticalBezierPan.Pan;
                float tangleTime = (tangentRight.anchoredPosition.x) / (_timeLineSettings.DistanceBetweenBeatLines + _timeLineKeyframeScroll.Pan) * (60f / _main.MusicDataSo.bpm);
                
                double nextTimeDelta = _timeLineConverter.TicksToSeconds(bezierPoint.NextKey.Ticks - _keyframe.Ticks);
                double tangleTimeDelta = _timeLineConverter.TicksToSeconds(_keyframe.Ticks) + tangleTime - _timeLineConverter.TicksToSeconds(_keyframe.Ticks);

                float weight = (float)(tangleTimeDelta / nextTimeDelta);

                _keyframe.OutTangent = tangleValue / tangleTime;
                _keyframe.OutWeight = weight; 
                
                _bezierLineDrawer.UpdateBezierCurve();
                bezierPointTangleLineDrawer.UpdatePosition();
            }
            
            if (_isDraggingTangleLeft)
            {

                
                print(tangentLeft.anchoredPosition);
                
                // Вычисляем новую позицию без учета смещения корня
                Vector2 position = tangentLeft.anchoredPosition - (tangentLeft.anchoredPosition - GetMousePosition(rectTransform));
                
                if(position.x > -0.1f) 
                    position = new Vector2(-0.1f, position.y);
                
                tangentLeft.anchoredPosition = position;

                float tangleValue = tangentLeft.anchoredPosition.y / _verticalBezierPan.Pan;
                float tangleTime = (tangentLeft.anchoredPosition.x) / (_timeLineSettings.DistanceBetweenBeatLines + _timeLineKeyframeScroll.Pan) * (60f / _main.MusicDataSo.bpm);
                
                double prevTimeDelta = _timeLineConverter.TicksToSeconds(_keyframe.Ticks - bezierPoint.PrevKey.Ticks);
                double tangleTimeDelta = _timeLineConverter.TicksToSeconds(_keyframe.Ticks) + tangleTime - _timeLineConverter.TicksToSeconds(_keyframe.Ticks);

                float weight = (float)(tangleTimeDelta / prevTimeDelta);

                _keyframe.InTangent = tangleValue / tangleTime;
                _keyframe.InWeight = Math.Abs(weight); 
                
                _bezierLineDrawer.UpdateBezierCurve();
                bezierPointTangleLineDrawer.UpdatePosition();
            }
        }
    }
}