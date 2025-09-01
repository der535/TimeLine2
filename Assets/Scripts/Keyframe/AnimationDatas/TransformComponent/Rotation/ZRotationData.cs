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

        public override AnimationData Interpolate(AnimationData other, double t)
        {
            ZRotationData otherPos = (ZRotationData)other;
            return new ZRotationData(Mathf.Lerp(value, otherPos.value, (float)t));
        }

        public override void Apply(GameObject target)
        {
            global::TimeLine.TransformComponent transformComponent = target.GetComponent<global::TimeLine.TransformComponent>();
            transformComponent.ZRotation.Value = value;
        }
    }
}