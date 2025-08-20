using UnityEngine.UIElements;

namespace TimeLine.Keyframe.AnimationDatas.TransformComponent
{
    using UnityEngine;

    [System.Serializable]
    public class XScaleData : AnimationData
    {
        public float value;

        public XScaleData(float value)
        {
            this.value = value;
        }

        public override AnimationData Clone()
        {
            return new XScaleData(value);
        }

        public override AnimationData Interpolate(AnimationData other, float t)
        {
            XScaleData otherPos = (XScaleData)other;
            return new XScaleData(Mathf.Lerp(value, otherPos.value, t));
        }

        public override void Apply(GameObject target)
        {
            global::TimeLine.TransformComponent transformComponent = target.GetComponent<global::TimeLine.TransformComponent>();
            transformComponent.XScale.Value = value;
        }
    }
}