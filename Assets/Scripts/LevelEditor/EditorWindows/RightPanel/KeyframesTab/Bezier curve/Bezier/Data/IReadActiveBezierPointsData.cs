using System.Collections.Generic;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.Data
{
    public interface IReadActiveBezierPointsData
    {
        public List<BezierPoint> Get();

        public BezierPoint GetFromKeyframe(global::TimeLine.Keyframe.Keyframe keyframe);
    }
}