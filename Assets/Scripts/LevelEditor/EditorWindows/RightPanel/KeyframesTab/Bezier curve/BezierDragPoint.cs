using System;
using EventBus;
using TimeLine.EventBus.Events.Bezier;
using TimeLine.Installers;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.Core.MusicData;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine;
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
        [Space] [SerializeField] private RectTransform rectTransform;
        [SerializeField] public RectTransform tangentLeft;
        [SerializeField] public RectTransform tangentRight;

        private TimeLineKeyframeZoom _timeLineKeyframeZoom;
        private VerticalBezierZoom _verticalBezierZoom;
        private TimeLineConverter _timeLineConverter;
        private BezierLineDrawer _bezierLineDrawer;
        private MainObjects _mainObjects;
        private BezierController _bezierController;
        private TimeLineSettings _timeLineSettings;
        private BezierSelectPointsController _bezierSelectPointsController;
        private Main _main;
        private M_MusicData _musicData;
        private ActionMap _actionMap;

        private Vector2 _startMousePosition;
        private Vector2 _startObjectPosition;
        private Action _sortKeyframes;

        private GameEventBus _gameEventBus;
        private GridUI _gridUI;
        private bool _isDragging;
        private bool _isDraggingTangleLeft;
        private bool _isDraggingTangleRight;

        public Keyframe.Keyframe _keyframe { get; private set; }
        public Keyframe.Keyframe _original { get; private set; }

        [Inject]
        private void Construct(
            MainObjects mainObject,
            TimeLineConverter timeLineConverter,
            GridUI gridUI,
            TimeLineKeyframeZoom timeLineKeyframeZoom,
            VerticalBezierZoom verticalBezierZoom,
            BezierLineDrawer bezierLineDrawer,
            BezierController bezierController,
            TimeLineSettings timeLineSettings,
            Main main,
            BezierSelectPointsController bezierSelectPointsController, M_MusicData musicData, GameEventBus gameEventBus, ActionMap actionMap)
        {
            _gridUI = gridUI;
            _mainObjects = mainObject;
            _timeLineConverter = timeLineConverter;
            _timeLineKeyframeZoom = timeLineKeyframeZoom;
            _verticalBezierZoom = verticalBezierZoom;
            _bezierLineDrawer = bezierLineDrawer;
            _bezierController = bezierController;
            _actionMap = actionMap;
            _timeLineSettings = timeLineSettings;
            _main = main;
            _bezierSelectPointsController = bezierSelectPointsController;
            _musicData = musicData;
            _gameEventBus = gameEventBus;
        }

        public void Setup(Keyframe.Keyframe keyframe, Keyframe.Keyframe original, Action sortKeyframes)
        {
            _keyframe = keyframe;
            _original = original;
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
            _gameEventBus.Raise(new DragTangentEvent(drag));
            _isDraggingTangleLeft = drag;
        }

        public void DragTangentRightPoint(bool drag)
        {
            _gameEventBus.Raise(new DragTangentEvent(drag));
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
                float rootOffset = _mainObjects.KeyframeRootRectTransform.offsetMin.x -
                                   _mainObjects.KeyframeScrollView.offsetMin.x -
                                   _mainObjects.KeyframeVerticalLayoutGroup.padding.left / 2f;

                float relativePosition = newPositionX - rootOffset;
                float roundedRelativePosition =
                    _gridUI.RoundAnchorPositionToGrid(relativePosition, _timeLineKeyframeZoom.Zoom);
                float finalPositionX = roundedRelativePosition + rootOffset;

                // Подробный принт X-позиции
                // Debug.Log($"[Drag X] StartObjX: {_startObjectPosition.x:F2}, StartMouseX: {_startMousePosition.x:F2}, MouseX: {GetMousePosition().x:F2} → " +
                //           $"NewX: {newPositionX:F2}, RootOffset: {rootOffset:F2}, RelPos: {relativePosition:F2}, RoundedRelPos: {roundedRelativePosition:F2}, FinalX: {finalPositionX:F2}");

                rectTransform.anchoredPosition = new Vector2(finalPositionX, rectTransform.anchoredPosition.y);

                // Вычисляем тики на основе относительной позиции
                var previosTick = _keyframe.Ticks;

                _keyframe.Ticks = MathF.Round((float)_timeLineConverter.SecondsToTicks(
                    _timeLineConverter.GetTimeFromAnchorPosition(roundedRelativePosition,
                        _timeLineKeyframeZoom.Zoom)));
                
                _original.Ticks = _keyframe.Ticks;

                var tickDifferent = _keyframe.Ticks - previosTick;


                _sortKeyframes?.Invoke();

                #endregion

                #region Vertical

                // Вычисляем новую позицию без учета смещения корня
                float newPositionY = _startObjectPosition.y - (_startMousePosition.y - GetMousePosition().y);

                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newPositionY);

                var previosValue = _keyframe.GetEntityData().GetValue();

                _keyframe.GetEntityData().SetValue(rectTransform.anchoredPosition.y / _verticalBezierZoom.Zoom);

                var difference = (float)_keyframe.GetEntityData().GetValue() - (float)previosValue;

                #endregion

                _bezierSelectPointsController.MultipleDrag(tickDifferent, difference, _original);

                _bezierLineDrawer.SortPoints();
                _bezierController.SortPoints();
                _bezierController.UpdatePositions();
                _bezierLineDrawer.UpdateBezierCurve();
                bezierPointTangleLineDrawer.UpdatePosition();
            }

            // Внутри BezierDragPoint.Update()
            if (_isDraggingTangleRight)
            {
                UpdateTangent(tangentRight, bezierPoint.NextKey, true);
            }

            if (_isDraggingTangleLeft)
            {
                UpdateTangent(tangentLeft, bezierPoint.PrevKey, false);
            }
        }
        private void UpdateTangent(RectTransform tangentTransform, Keyframe.Keyframe goalKey,  bool isRight)
        {
            Vector2 localMousePos = GetMousePosition(rectTransform);
            Vector2 newPosition = localMousePos;

            float pan = _timeLineSettings.DistanceBetweenBeatLines + _timeLineKeyframeZoom.Zoom;
            float bpmFactor = _musicData.bpm / 60f;
            float unitsPerPixel = pan * bpmFactor;

            // Расчет временных интервалов
            double targetTimeSec = _timeLineConverter.TicksToSeconds(goalKey.Ticks);
            double currentTimeSec = _timeLineConverter.TicksToSeconds(_keyframe.Ticks);
            double deltaTimeSec = Math.Abs(targetTimeSec - currentTimeSec);
            float maxX = (float)(deltaTimeSec * unitsPerPixel);

            // Ограничения и нормализация направления
            if (isRight)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, 0.1f, maxX);
            }
            else
            {
                newPosition.x = Mathf.Clamp(newPosition.x, -maxX, -0.1f);
            }
            
            if(_actionMap.Editor.LeftShift.IsPressed())
                _bezierController.TangleUpdate(tangentTransform.anchoredPosition-newPosition, this, isRight);
            
            tangentTransform.anchoredPosition = newPosition;

            // Расчет смещений (делаем их абсолютными для времени, чтобы избежать путаницы со знаками)
            float absTimeOffset = Mathf.Abs(newPosition.x) / unitsPerPixel;
            float valueOffset = isRight ? newPosition.y : -newPosition.y; 
            float verticalZoom = _verticalBezierZoom.Zoom;

            float weight = Mathf.Clamp01((float)(absTimeOffset / deltaTimeSec));
            float tangent = (valueOffset / verticalZoom) / absTimeOffset;

            // Применяем значения
            if (isRight)
            {
                _keyframe.OutWeight = _original.OutWeight = weight;
                _keyframe.OutTangent = _original.OutTangent = tangent;
            }
            else
            {
                _keyframe.InWeight = _original.InWeight = weight;
                _keyframe.InTangent = _original.InTangent = tangent;
            }

            // Обновление визуала
            _bezierLineDrawer.UpdateBezierCurve();
            bezierPointTangleLineDrawer.UpdatePosition();
        }
        
        
        public void UpdateTangent(RectTransform tangentTransform, Keyframe.Keyframe goalKey, bool isRight, Vector2 deltaTangent, BezierDragPoint self)
        {
            if(self == this) return;
            
            Vector2 newPosition = tangentTransform.anchoredPosition - deltaTangent;

            float pan = _timeLineSettings.DistanceBetweenBeatLines + _timeLineKeyframeZoom.Zoom;
            float bpmFactor = _musicData.bpm / 60f;
            float unitsPerPixel = pan * bpmFactor;

            Debug.Log(goalKey);
            Debug.Log(goalKey.Ticks);
            // Расчет временных интервалов
            double targetTimeSec = _timeLineConverter.TicksToSeconds(goalKey.Ticks);
            double currentTimeSec = _timeLineConverter.TicksToSeconds(_keyframe.Ticks);
            double deltaTimeSec = Math.Abs(targetTimeSec - currentTimeSec);
            float maxX = (float)(deltaTimeSec * unitsPerPixel);

            // Ограничения и нормализация направления
            if (isRight)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, 0.1f, maxX);
            }
            else
            {
                newPosition.x = Mathf.Clamp(newPosition.x, -maxX, -0.1f);
            }

            tangentTransform.anchoredPosition = newPosition;

            // Расчет смещений (делаем их абсолютными для времени, чтобы избежать путаницы со знаками)
            float absTimeOffset = Mathf.Abs(newPosition.x) / unitsPerPixel;
            float valueOffset = isRight ? newPosition.y : -newPosition.y; 
            float verticalZoom = _verticalBezierZoom.Zoom;

            float weight = Mathf.Clamp01((float)(absTimeOffset / deltaTimeSec));
            float tangent = (valueOffset / verticalZoom) / absTimeOffset;

            // Применяем значения
            if (isRight)
            {
                _keyframe.OutWeight = _original.OutWeight = weight;
                _keyframe.OutTangent = _original.OutTangent = tangent;
            }
            else
            {
                _keyframe.InWeight = _original.InWeight = weight;
                _keyframe.InTangent = _original.InTangent = tangent;
            }

            // Обновление визуала
            _bezierLineDrawer.UpdateBezierCurve();
            bezierPointTangleLineDrawer.UpdatePosition();
        }
    }
}