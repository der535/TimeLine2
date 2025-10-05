namespace TimeLine.Keyframe
{
    using UnityEngine;

    public class Keyframe
    {
        public double Ticks { get; set; }
        
        public double OutTangent { get; set; }
        public double InTangent { get; set; }
        public double InWeight { get; set; }
        public double OutWeight { get; set; }
        
        private AnimationData animationData;

        public Keyframe(double ticks, double outTangent = 0, double inTangent = 0, double inWeight = 0.5f, double outWeight = 0.5f)
        {
            this.Ticks = Mathf.Round((float)ticks);

            OutTangent = outTangent;
            InTangent = inTangent;
            InWeight = inWeight;
            OutWeight = outWeight;
        }

        public void AddData(AnimationData data)
        {
            animationData = data;
        }

        public void Apply(GameObject target)
        {
            // Debug.Log(target);
            animationData.Apply(target);
        }
        
        public AnimationData GetData() => animationData;

        public Keyframe Clone()
        {
            Keyframe clone = new Keyframe(Ticks, OutTangent, InTangent, InWeight, OutWeight);
            clone.AddData(animationData.Clone());
            return clone;
        }

        public void Interpolate(Keyframe next, GameObject target, double t)
        {
            AnimationData currentData = animationData;
            AnimationData nextData = next.animationData;

            if (currentData != null && nextData != null)
            {
                currentData.Interpolate(nextData, t, this,next).Apply(target);
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