namespace TimeLine.Keyframe
{
    using UnityEngine;

    public abstract class AnimationData
    {
        public abstract AnimationData Interpolate(AnimationData other, float t);
        public abstract void Apply(GameObject target);
    
        // Для кастомных параметров
        public virtual void ApplyToComponent(Component component) { }
    }
}