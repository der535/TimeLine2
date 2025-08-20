namespace TimeLine.Keyframe.AnimationDatas.TransformComponent
{
    using UnityEngine;

    [System.Serializable]
    public class YScaleData : AnimationData
    {
        public float value;

        public YScaleData(float value)
        {
            this.value = value;
        }

        public override AnimationData Clone()
        {
            return new YScaleData(value);
        }

        public override AnimationData Interpolate(AnimationData other, float t)
        {
            YScaleData otherPos = (YScaleData)other;
            return new YScaleData(Mathf.Lerp(value, otherPos.value, t));
        }

        public override void Apply(GameObject target)
        {
            global::TimeLine.TransformComponent transformComponent = target.GetComponent<global::TimeLine.TransformComponent>();
            transformComponent.YScale.Value = value;
        }
    }
}