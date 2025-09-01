namespace TimeLine.Keyframe.AnimationDatas.TransformComponent
{
    using UnityEngine;

    [System.Serializable]
    public class YRotationData : AnimationData
    {
        public float value;

        public YRotationData(float value)
        {
            this.value = value;
        }

        public override AnimationData Clone()
        {
            return new YRotationData(value);
        }

        public override AnimationData Interpolate(AnimationData other, double t)
        {
            YRotationData otherPos = (YRotationData)other;
            return new YRotationData(Mathf.Lerp(value, otherPos.value, (float)t));
        }

        public override void Apply(GameObject target)
        {
            global::TimeLine.TransformComponent transformComponent = target.GetComponent<global::TimeLine.TransformComponent>();
            transformComponent.YRotation.Value = value;
        }
    }
}