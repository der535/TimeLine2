using System.Collections.Generic;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.Data
{
    public class ActiveBezierPointsData : IReadActiveBezierPointsData
    {
        public List<BezierPoint> Value = new();

        public BezierPoint GetFromKeyframe(global::TimeLine.Keyframe.Keyframe keyframe)
        {
            return Value.Find(x => x.BezierDragPoint._original == keyframe);
        }
        
        public void DeselectAll()
        {
            foreach (var point in Value)
            {
                point.BezierSelectPoint.Deselect();
            }
        }
        
        public List<BezierPoint> Get()
        {
            return Value;
        }
    }
}