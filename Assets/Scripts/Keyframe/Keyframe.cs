namespace TimeLine.Keyframe
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class Keyframe
    {
        public double ticks; // Время в тиках
        private AnimationData animationData;

        public Keyframe(double ticks)
        {
            this.ticks = Mathf.Round((float)ticks);
        }

        public void AddData(AnimationData data)
        {
            animationData = data;
        }

        public void Apply(GameObject target)
        {
            animationData.Apply(target);
        }
        
        public AnimationData GetData() => animationData;

        public Keyframe Clone()
        {
            Keyframe clone = new Keyframe(ticks);
            animationData.Clone();
            return clone;
        }

        public void Interpolate(Keyframe next, GameObject target, double t)
        {
            AnimationData currentData = animationData;
            AnimationData nextData = next.animationData;

            if (currentData != null && nextData != null)
            {
                currentData.Interpolate(nextData, t).Apply(target);
            }
            else if (currentData != null)
            {
                currentData.Apply(target);
            }
            else if (nextData != null)
            {
                nextData.Apply(target);
            }
        }
    }
}