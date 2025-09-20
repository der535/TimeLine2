using System;
using TimeLine.Installers;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.Input
{
    public class CursorBeatPosition : MonoBehaviour
    {
        [SerializeField] private TimeLineSettings timeLineSettings;
        [FormerlySerializedAs("gridSystem")] [SerializeField] private GridUI gridUI;

        private bool _isActive;
        private bool _dragStarted; // Флаг: начали ли мы перетаскивание при _isActive == true

        private Main _main;
        private MainObjects _mainObjects;
        private TimeLineScroll _timeLineScroll;

        [Inject]
        private void Construct(Main main, MainObjects mainObjects, TimeLineScroll timeLineScroll)
        {
            _main = main;
            _timeLineScroll = timeLineScroll;
            _mainObjects = mainObjects;
        }

        public void SetActive(bool isActive) => _isActive = isActive;

        public Vector2 GetCursorPosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_mainObjects.CanvasRectTransform,
                UnityEngine.Input.mousePosition, _mainObjects.MainCamera, out var vector2);
            return vector2;
        }

        private void Update()
        {
            bool isMouseHeld = UnityEngine.Input.GetKey(KeyCode.Mouse0);

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
                }
            }
        }

        private void UpdateCursorPosition()
        {
            // Получаем позицию курсора
            Vector2 cursorPos = GetCursorPosition();
            float pixelX = cursorPos.x - _mainObjects.ContentRectTransform.offsetMin.x;

            // Вычисляем позицию в тиках
            double ticksPerPixel = Main.TICKS_PER_BEAT / (timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan);
            double rawTicks = pixelX * ticksPerPixel;

            // Округляем до сетки
            double gridSizeInTicks = gridUI.GetGridSizeInTicks();
            double roundedTicks = Math.Round(rawTicks / gridSizeInTicks) * gridSizeInTicks;

            // Устанавливаем время
            _main.SetTimeInTicks(Math.Max(0, roundedTicks));
        }
    }
}