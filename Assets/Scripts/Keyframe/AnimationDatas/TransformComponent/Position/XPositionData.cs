using TimeLine.TimeLine;

namespace TimeLine.Keyframe.AnimationDatas.TransformComponent
{
    using UnityEngine;

    [System.Serializable]
    public class XPositionData : AnimationData
    {
        public float value;

        public XPositionData(float value)
        {
            this.value = value;
        }

        public override AnimationData Clone()
        {
            return new XPositionData(value);
        }

        public override object GetValue()
        {
            return value;
        }

        public override void SetValue(object value)
        {
            if(value is float f) this.value = f;
            else
            {
                Debug.LogWarning("[TimeLine.Keyframe] Cannot set XPositionData value to a float");
            }
        }

        public override AnimationData Interpolate(
            AnimationData other, 
            double t, 
            global::TimeLine.Keyframe.Keyframe current, 
            global::TimeLine.Keyframe.Keyframe next)
        {
            if (other is not XPositionData otherPos)
                throw new System.ArgumentException("Interpolation requires another XPositionData.");

            float localT = (float)t;
            float interpolatedValue = TimeLineConverter.Instance.Interpolate(
                value,
                otherPos.value,
                current,
                next,
                localT
            );

            return new XPositionData(interpolatedValue);
        }
        
        
        public override void Apply(GameObject target)
        {
            global::TimeLine.TransformComponent transformComponent = target.GetComponent<global::TimeLine.TransformComponent>();
            transformComponent.XPosition.Value = value;
        }
    }
}