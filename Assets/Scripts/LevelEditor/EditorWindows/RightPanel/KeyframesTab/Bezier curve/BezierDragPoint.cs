using System;
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
        [SerializeField] private RectTransform tangentLeft;
        [SerializeField] private RectTransform tangentRight;

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

        private Vector2 _startMousePosition;
        private Vector2 _startObjectPosition;
        private Action _sortKeyframes;

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
            BezierSelectPointsController bezierSelectPointsController, M_MusicData musicData)
        {
            _gridUI = gridUI;
            _mainObjects = mainObject;
            _timeLineConverter = timeLineConverter;
            _timeLineKeyframeZoom = timeLineKeyframeZoom;
            _verticalBezierZoom = verticalBezierZoom;
            _bezierLineDrawer = bezierLineDrawer;
            _bezierController = bezierController;
            _timeLineSettings = timeLineSettings;
            _main = main;
            _bezierSelectPointsController = bezierSelectPointsController;
            _musicData = musicData;
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
                Vector2 localMousePos = GetMousePosition(rectTransform);
                Vector2 newPosition = localMousePos; // Прямое позиционирование вместо сложных вычислений

                // ===== ИСПРАВЛЕННЫЕ ОГРАНИЧЕНИЯ =====
                float pan = _timeLineSettings.DistanceBetweenBeatLines + _timeLineKeyframeZoom.Zoom;
                float bpmFactor = _musicData.bpm / 60f;

                // Минимальное смещение (0.1f) остаётся
                newPosition.x = Mathf.Max(0.1f, newPosition.x);

                // Максимальное смещение в пикселях для правого уса
                double nextTimeSec = _timeLineConverter.TicksToSeconds(bezierPoint.NextKey.Ticks);
                double currentTimeSec = _timeLineConverter.TicksToSeconds(_keyframe.Ticks);
                double deltaTimeSec = nextTimeSec - currentTimeSec;
                float maxX = (float)(deltaTimeSec * pan * bpmFactor); // Правильный расчёт максимума

                newPosition.x = Mathf.Min(maxX, newPosition.x);
                tangentRight.anchoredPosition = newPosition;

                // ===== ИСПРАВЛЕННЫЕ ВЫЧИСЛЕНИЯ =====
                float outTimeOffset = newPosition.x / (pan * bpmFactor); // Время в секундах
                float outValueOffset = newPosition.y / _verticalBezierZoom.Zoom; // Значение в юнитах

                // Вес: отношение смещения к общему времени
                _keyframe.OutWeight = Mathf.Clamp01((float)(outTimeOffset / deltaTimeSec));
                _original.OutWeight = Mathf.Clamp01((float)(outTimeOffset / deltaTimeSec));

                // Тангенс: отношение изменения значения ко времени
                _keyframe.OutTangent = outValueOffset / outTimeOffset; // Без деления на deltaTime!
                _original.OutTangent = outValueOffset / outTimeOffset;

                // Обновление визуала
                _bezierLineDrawer.UpdateBezierCurve();
                bezierPointTangleLineDrawer.UpdatePosition();
            }

            if (_isDraggingTangleLeft)
            {
                Vector2 localMousePos = GetMousePosition(rectTransform);
                Vector2 newPosition = localMousePos;

                float pan = _timeLineSettings.DistanceBetweenBeatLines + _timeLineKeyframeZoom.Zoom;
                float bpmFactor = _musicData.bpm / 60f;

                // Для левого уса: координаты отрицательные
                newPosition.x = Mathf.Min(-0.1f, newPosition.x); // Максимум -0.1f (ближе к нулю)

                double prevTimeSec = _timeLineConverter.TicksToSeconds(bezierPoint.PrevKey.Ticks);
                double currentTimeSec = _timeLineConverter.TicksToSeconds(_keyframe.Ticks);
                double deltaTimeSec = currentTimeSec - prevTimeSec;
                float maxX = (float)(deltaTimeSec * pan * bpmFactor); // Максимальное смещение по модулю

                newPosition.x = Mathf.Max(-maxX, newPosition.x); // Ограничение по модулю
                tangentLeft.anchoredPosition = newPosition;

                // ===== ОСОБЕННОСТИ ЛЕВОГО УСА =====
                // В Setup(): x = -(inTimeOffset * pan * bpmFactor)
                float inTimeOffset = -newPosition.x / (pan * bpmFactor); // Делаем положительным
                float inValueOffset = -newPosition.y / _verticalBezierZoom.Zoom; // Знак зависит от направления

                _keyframe.InWeight = Mathf.Clamp01((float)(inTimeOffset / deltaTimeSec));
                _keyframe.InTangent = inValueOffset / inTimeOffset;
                
                _original.InWeight = Mathf.Clamp01((float)(inTimeOffset / deltaTimeSec));
                _original.InTangent = inValueOffset / inTimeOffset;

                _bezierLineDrawer.UpdateBezierCurve();
                bezierPointTangleLineDrawer.UpdatePosition();
            }
        }
    }
}