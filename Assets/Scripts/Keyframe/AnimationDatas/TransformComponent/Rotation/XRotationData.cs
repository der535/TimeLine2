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

        public override object GetValue()
        {
            return value;
        }

        public override AnimationData Interpolate(AnimationData other, double t)
        {
            XRotationData otherPos = (XRotationData)other;
            return new XRotationData(Mathf.Lerp(value, otherPos.value, (float)t));
        }

        public override void Apply(GameObject target)
        {
            global::TimeLine.TransformComponent transformComponent = target.GetComponent<global::TimeLine.TransformComponent>();
            transformComponent.XRotation.Value = value;
        }
    }
}