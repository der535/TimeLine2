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

        public override AnimationData Interpolate(AnimationData other, float t)
        {
            XPositionData otherPos = (XPositionData)other;
            return new XPositionData(Mathf.Lerp(value, otherPos.value, t));
        }

        public override void Apply(GameObject target)
        {
            target.GetComponent<global::TimeLine.TransformComponent>().XPosition = this.value;
        }
    }
}