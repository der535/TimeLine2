using System;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.Input
{
    public class KeyframeMoveTimeLine : MonoBehaviour
    {
        [SerializeField] private RectTransform clickArea;
        [SerializeField] private TabStorage tabStorage;
        [SerializeField] private RectTransform scrollingRecTransform;
        [FormerlySerializedAs("camera")] [SerializeField] private Camera _camera;

        [FormerlySerializedAs("gridSystem")] [SerializeField]
        private GridUI gridUI;

        [SerializeField] private float snapingRange = 20;

        private bool _isActive;
        private bool _dragStarted; // Флаг: начали ли мы перетаскивание при _isActive == true

        private Main _main;
        private ActionMap _actionMap;
        private TrackObjectStorage _trackObjectStorage;
        private SelectObjectController _selectObjectController;
        private KeyframeVizualizer _keyframeVizualizer;
        private TimeLineKeyframeZoom _timeLineKeyframeZoom;

        [Inject]
        private void Construct(Main main, MainObjects mainObjects, TimeLineScroll timeLineScroll, ActionMap actionMap,
            TrackObjectStorage trackObjectStorage, TimeLineKeyframeZoom timeLineKeyframeZoom,
            KeyframeVizualizer keyframeVizualizer, SelectObjectController selectObjectController)
        {
            _main = main;
            _actionMap = actionMap;
            _keyframeVizualizer = keyframeVizualizer;
            _timeLineKeyframeZoom = timeLineKeyframeZoom;
            _trackObjectStorage = trackObjectStorage;
            _selectObjectController = selectObjectController;
        }

        public Vector2 GetCursorPosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(scrollingRecTransform,
                UnityEngine.Input.mousePosition, _camera, out var vector2);
            return vector2;
        }

        public void ClickArea()
        {
            // 1. Переводим позицию мыши в локальные координаты RectTransform
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    clickArea,
                    UnityEngine.Input.mousePosition,
                    _camera,
                    out var localPoint))
            {
                // 2. Проверяем, входит ли локальная точка в границы прямоугольника
                if (clickArea.rect.Contains(localPoint) && clickArea.transform.gameObject.activeInHierarchy && UnityEngine.Input.GetMouseButtonDown(0))
                {
                    _isActive = true;
                }
            }
        }

        private void Update()
        {
            ClickArea();
            bool isMouseHeld = _actionMap.Editor.MouseLeft.IsPressed();

            
            
            if (!_dragStarted)
            {
                // Ждём одновременного выполнения: активен + нажата мышь
                if (_isActive && isMouseHeld)
                {
                    _dragStarted = true;
                    UpdateCursorPosition();
                }
            }
            else
            {
                // Перетаскивание уже начато — обновляем, пока мышь удерживается
                if (isMouseHeld)
                {
                    UpdateCursorPosition();
                }
                else
                {
                    // Мышь отпущена — сбрасываем флаг, возвращаемся к строгому режиму
                    _dragStarted = false;
                    _isActive = false;
                }
            }
        }

        private void UpdateCursorPosition()
        {
            Vector2 cursorPos = GetCursorPosition();
            float pixelX = cursorPos.x;

            
            double ticksPerPixel = TimeLineConverter.TICKS_PER_BEAT / (_timeLineKeyframeZoom.Zoom);
            double rawTicks = pixelX * ticksPerPixel + _selectObjectController.SelectObjects[^1].components.Data.StartTimeInTicks;

            // 1. Сетка работает всегда (базовое поведение)
            double gridSizeInTicks = gridUI.GetGridSizeInTicks();
            double targetTicks = Math.Round(rawTicks / gridSizeInTicks) * gridSizeInTicks;

            // 2. ПРИВЯЗКА (СНАППИНГ) — добавляем условие зажатой клавиши

            if (_actionMap.Editor.LeftShift.IsPressed())
            {
                double snapThresholdTicks = snapingRange * ticksPerPixel;

                if (TryGetSnapTicks(rawTicks, snapThresholdTicks, out double snappedPoint))
                {
                    targetTicks = snappedPoint;
                }
            }
            

            _main.SetTimeInTicks(targetTicks, true);
        }

        private bool TryGetSnapTicks(double currentTicks, double threshold, out double finalTicks)
        {
            double bestPoint = 0;
            double minDistance = threshold;
            bool found = false;

            // Используем локальные переменные (bestPoint, minDistance, found) 
            // внутри локальной функции Check — это разрешено.
            void Check(double targetPoint)
            {
                double distance = Math.Abs(currentTicks - targetPoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestPoint = targetPoint;
                    found = true;
                }
            }

            foreach (var wrap in _keyframeVizualizer.GetAllKeyframesList())
                Check(wrap.Ticks + _selectObjectController.SelectObjects[^1].components.Data.StartTimeInTicks);


            // Только в самом конце присваиваем результат out параметру
            finalTicks = bestPoint;
            return found;
        }
    }
}