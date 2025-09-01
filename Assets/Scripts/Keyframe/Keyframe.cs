namespace TimeLine.Keyframe
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class Keyframe
    {
        public double ticks; // Время в тиках
        private List<AnimationData> animationData = new List<AnimationData>();

        public Keyframe(double ticks)
        {
            this.ticks = ticks;
        }
        
        public void AddData(AnimationData data)
        {
            animationData.Add(data);
        }

        public void Apply(GameObject target)
        {
            foreach (AnimationData data in animationData)
            {
                data.Apply(target);
            }
        }
        
        public Keyframe Clone()
        {
            Keyframe clone = new Keyframe(ticks);
            foreach (AnimationData data in animationData)
            {
                clone.animationData.Add(data.Clone());
            }
            return clone;
        }

        public void Interpolate(Keyframe next, GameObject target, double t)
        {
            // Для каждого типа данных
            var allTypes = animationData.Select(d => d.GetType())
                .Union(next.animationData.Select(d => d.GetType()))
                .Distinct();
            
            foreach (System.Type type in allTypes)
            {
                AnimationData currentData = animationData.FirstOrDefault(d => d.GetType() == type);
                AnimationData nextData = next.animationData.FirstOrDefault(d => d.GetType() == type);
            
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
}