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

        public override AnimationData Interpolate(AnimationData other, float t)
        {
            XPositionData otherPos = (XPositionData)other;
            return new XPositionData(Mathf.Lerp(value, otherPos.value, t));
        }

        public override void Apply(GameObject target)
        {
            global::TimeLine.TransformComponent transformComponent = target.GetComponent<global::TimeLine.TransformComponent>();
            transformComponent.XPosition.Value = value;
        }
    }
}