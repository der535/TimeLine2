using TimeLine.LevelEditor.Core;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.Service
{
    public class BezierCursorValue
    {
        private KeyframeReferences _keyframeReferences;
        private CameraReferences _cameraReferences;
        
        [Inject]
        private void Construct(KeyframeReferences keyframeReferences, CameraReferences cameraReferences)
        {
            _keyframeReferences = keyframeReferences;
            _cameraReferences = cameraReferences;
        }
        
        public float GetCursorValuePosition(float pan)
        {
            float scrollFactor = pan;
            if (Mathf.Approximately(scrollFactor, 0f)) return 0f;

            // Получаем позицию курсора ОТНОСИТЕЛЬНО КОРНЯ (root)
            Vector2 cursorLocalPos = GetCursorPosition();
            float cursorInRootSpace = cursorLocalPos.y - _keyframeReferences.rootPoints.anchoredPosition.y; // ← ВАЖНО!

            float value = cursorInRootSpace / scrollFactor;


            return value;
        }

        private Vector2 GetCursorPosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_keyframeReferences.rootObjects,
                UnityEngine.Input.mousePosition, _cameraReferences.editUICamera, out var localPoint);
            return localPoint;
        }
    }
}