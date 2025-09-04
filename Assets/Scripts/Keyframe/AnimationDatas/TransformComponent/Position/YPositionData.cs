using UnityEngine;

namespace TimeLine.Keyframe.AnimationDatas.TransformComponent.Position
{
    [System.Serializable]
    public class YPositionData : AnimationData
    {
        public float value;

        public YPositionData(float value)
        {
            this.value = value;
        }

        public override AnimationData Clone()
        {
            return new YPositionData(value);
        }

        public override object GetValue()
        {
            return value;
        }

        public override AnimationData Interpolate(AnimationData other, double t)
        {
            YPositionData otherPos = (YPositionData)other;
            return new YPositionData(Mathf.Lerp(value, otherPos.value, (float)t));
        }

        public override void Apply(GameObject target)
        {
            global::TimeLine.TransformComponent transformComponent = target.GetComponent<global::TimeLine.TransformComponent>();
            transformComponent.YPosition.Value = value;
        }
    }
}