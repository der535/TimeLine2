namespace TimeLine.Keyframe.AnimationDatas.TransformComponent
{
    using UnityEngine;

    [System.Serializable]
    public class XRotationData : AnimationData
    {
        public float value;

        public XRotationData(float value)
        {
            this.value = value;
        }

        public override AnimationData Clone()
        {
            return new XRotationData(value);
        }

        public override AnimationData Interpolate(AnimationData other, float t)
        {
            XRotationData otherPos = (XRotationData)other;
            return new XRotationData(Mathf.Lerp(value, otherPos.value, t));
        }

        public override void Apply(GameObject target)
        {
            global::TimeLine.TransformComponent transformComponent = target.GetComponent<global::TimeLine.TransformComponent>();
            transformComponent.XRotation.Value = value;
        }
    }
}