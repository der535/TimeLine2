using System;
using TimeLine.Installers;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.Input
{
    public class KeyframeMoveTimeLine : MonoBehaviour
    {
        [SerializeField] private RectTransform clickArea;
        [SerializeField] private RectTransform scrollingRecTransform;
        [SerializeField] private Camera camera;

        [FormerlySerializedAs("gridSystem")] [SerializeField]
        private GridUI gridUI;

        [SerializeField] private float snapingRange = 20;

        private bool _isActive;
        private bool _dragStarted; // Флаг: начали ли мы перетаскивание при _isActive == true

        private Main _main;
        private ActionMap _actionMap;
        private TrackObjectStorage _trackObjectStorage;
        private TimeLineKeyframeScroll _timeLineKeyframeScroll;

        [Inject]
        private void Construct(Main main, MainObjects mainObjects, TimeLineScroll timeLineScroll, ActionMap actionMap,
            TrackObjectStorage trackObjectStorage, TimeLineKeyframeScroll timeLineKeyframeScroll)
        {
            _main = main;
            _actionMap = actionMap;
            _trackObjectStorage = trackObjectStorage;
            _timeLineKeyframeScroll = timeLineKeyframeScroll;
        }

        public Vector2 GetCursorPosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(scrollingRecTransform,
                UnityEngine.Input.mousePosition, camera, out var vector2);
            return vector2;
        }
        public void ClickArea()
        {
            // 1. Переводим позицию мыши в локальные координаты RectTransform
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    clickArea, 
                    UnityEngine.Input.mousePosition, 
                    camera, 
                    out var localPoint))
            {
                // 2. Проверяем, входит ли локальная точка в границы прямоугольника
                if (clickArea.rect.Contains(localPoint) && UnityEngine.Input.GetMouseButtonDown(0))
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
            
            print(cursorPos.x);
            print(scrollingRecTransform.offsetMin.x);

            double ticksPerPixel = TimeLineConverter.TICKS_PER_BEAT / (_timeLineKeyframeScroll.Zoom);
            print(ticksPerPixel);
            double rawTicks = pixelX * ticksPerPixel;
            print(rawTicks);

            // 1. Сетка работает всегда (базовое поведение)
            double gridSizeInTicks = gridUI.GetGridSizeInTicks();
            double targetTicks = Math.Round(rawTicks / gridSizeInTicks) * gridSizeInTicks;

            // 2. ПРИВЯЗКА (СНАППИНГ) — добавляем условие зажатой клавиши
            // Используйте KeyCode.LeftControl, KeyCode.LeftShift или любую другую
            
            //todo Заменить на ключевые кадры
            //
            // if (_actionMap.Editor.LeftShift.IsPressed()) 
            // {
            //     double snapThresholdTicks = snapingRange * ticksPerPixel; 
            //
            //     if (TryGetSnapTicks(rawTicks, snapThresholdTicks, out double snappedPoint))
            //     {
            //         targetTicks = snappedPoint;
            //     }
            // }

            _main.SetTimeInTicks(targetTicks);
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

            void Process(double start, double duration)
            {
                Check(start); // Магнит к началу
                Check(start + duration - 1); // Магнит к концу (с учетом вычета)
            }

            // Проходим по всем объектам в хранилище
            if (_trackObjectStorage.TrackObjects != null)
            {
                foreach (var wrap in _trackObjectStorage.TrackObjects)
                    Process(wrap.trackObject.StartTimeInTicks, wrap.trackObject.TimeDuractionInTicks); //
            }

            // Проходим по всем группам
            if (_trackObjectStorage.TrackObjectGroups != null)
            {
                foreach (var wrap in _trackObjectStorage.TrackObjectGroups)
                    Process(wrap.trackObject.StartTimeInTicks, wrap.trackObject.TimeDuractionInTicks);
            }

            // Только в самом конце присваиваем результат out параметру
            finalTicks = bestPoint;
            return found;
        }
    }
}