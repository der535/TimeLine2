using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject
{
    public static class MousePosition 
    {
        /// <summary>
        /// Получение позиции курсора
        /// </summary>
        /// <param name="rectTransform">Рект трансформ канваса в пронстранстве которого будет получения позиция курсора</param>
        /// <param name="camera">Камера редактора</param>
        /// <returns></returns>
        public static Vector2 GetMousePosition(RectTransform rectTransform, Camera camera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                Mouse.current.position.ReadValue(),
                camera,
                out var currentLocalPosition);
            return currentLocalPosition;
        }
    }
}