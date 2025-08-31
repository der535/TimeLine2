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
            if (UnityEngine.Input.GetKey(KeyCode.Mouse0) && _isActive)
            {
                // Получаем позицию курсора
                Vector2 cursorPos = GetCursorPosition();
                print(cursorPos);
                float pixelX = cursorPos.x - _mainObjects.ContentRectTransform.offsetMin.x;
                
                // Вычисляем позицию в тиках
                double ticksPerPixel = Main.TICKS_PER_BEAT / (timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan);
                double rawTicks = pixelX * ticksPerPixel;
                
                print(rawTicks);
                
                // Округляем до сетки
                double gridSizeInTicks = gridUI.GetGridSizeInTicks();
                double roundedTicks = Math.Round(rawTicks / gridSizeInTicks) * gridSizeInTicks;
                
                print(roundedTicks);
                
                // Устанавливаем время
                _main.SetTimeInTicks(roundedTicks);
            }
        }
    }
}