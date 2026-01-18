using TimeLine.LevelEditor.Core;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.View
{
    public class BezierVerticalPosition
    {
        private KeyframeReferences _keyframeReferences;
        
        [Inject]
        private void Construct(KeyframeReferences keyframeReferences)
        {
            _keyframeReferences = keyframeReferences;
        }
        
        public void SetPosition(float position)
        {
            // Защита от NaN/Infinity
            if (float.IsNaN(position) || float.IsInfinity(position))
            {
                return;
            }

            _keyframeReferences.rootPoints.offsetMax = new Vector2(0, position);
            _keyframeReferences.rootPoints.offsetMin = new Vector2(0, position);
        }
    }
}