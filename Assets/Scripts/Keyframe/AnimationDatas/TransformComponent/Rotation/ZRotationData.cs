using TimeLine.TimeLine;
using UnityEngine;

namespace TimeLine.Keyframe.AnimationDatas.TransformComponent.Rotation
{
    [System.Serializable]
    public class ZRotationData : AnimationData
    {
        public float value;

        public ZRotationData(float value)
        {
            this.value = value;
        }

        public override AnimationData Clone()
        {
            return new ZRotationData(value);
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
        public override AnimationData Interpolate(AnimationData other, double t, Keyframe current, Keyframe next)
        {
            if (other is not ZRotationData otherPos)
                throw new System.ArgumentException("Interpolation requires another XPositionData.");

            float localT = (float)t;
            float interpolatedValue = TimeLineConverter.Instance.Interpolate(
                value,
                otherPos.value,
                current,
                next,
                localT
            );

            return new ZRotationData(interpolatedValue);
        }

        public override void Apply(GameObject target)
        {
            global::TimeLine.TransformComponent transformComponent = target.GetComponent<global::TimeLine.TransformComponent>();
            transformComponent.ZRotation.Value = value;
        }
    }
}