namespace TimeLine.Keyframe
{
    using UnityEngine;

    public abstract class AnimationData
    {
        public abstract AnimationData Interpolate(AnimationData other, double t);
        public abstract void Apply(GameObject target);
        
        // Добавленный абстрактный метод для клонирования
        public abstract AnimationData Clone();
        public abstract object GetValue();
    
        // Для кастомных параметров
        public virtual void ApplyToComponent(Component component) { }
    }
}