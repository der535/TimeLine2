using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;

namespace TimeLine.Keyframe
{
    public interface IAnimatable
    {
        AnimationData GetAnimationData();
        void ApplyAnimationData(AnimationData data);
    }
}