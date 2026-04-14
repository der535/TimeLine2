using TimeLine.LevelEditor.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeLine.LevelEditor.TransformationSquare
{
    /// <summary>
    /// Класс дающий позицию мышки в координатах anchor position в системе координат самой линии (_view.lineRenderer)
    /// </summary>
    public class TransformationSquareMousePosition
    {
        private TransformationSquareView _view;
        private CameraReferences _cameraReferences;
        
        public TransformationSquareMousePosition(TransformationSquareView view, CameraReferences cameraReferences)
        {
            _view = view;
            _cameraReferences = cameraReferences;
        }
        
        /// <summary>
        /// Метод дающий позицию мышки в anchor position в системе координат самой линии (_view.lineRenderer)
        /// </summary>
        /// <returns>Позиция мышки</returns>
        public Vector2 Get()
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)_view.lineRenderer.transform,
                    Mouse.current.position.ReadValue(),
                    _cameraReferences.editUICamera,
                    out var localPosition))
            {
                return localPosition;
            }

            return Vector2.zero;
        }
    }
}