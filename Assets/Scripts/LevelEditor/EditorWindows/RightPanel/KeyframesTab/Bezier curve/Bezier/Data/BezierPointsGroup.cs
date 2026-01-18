using System.Collections.Generic;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.Data
{
    internal class BezierPointsGroup
    {
        public List<BezierPoint> _keyframes = new();
        public List<global::TimeLine.Keyframe.Keyframe> _track;
    }
}