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
        public override AnimationData Interpolate(AnimationData other, double t)
        {
            YScaleData otherPos = (YScaleData)other;
            return new YScaleData(Mathf.Lerp(value, otherPos.value, (float)t));
        }

        public override void Apply(GameObject target)
        {
            global::TimeLine.TransformComponent transformComponent = target.GetComponent<global::TimeLine.TransformComponent>();
            transformComponent.YScale.Value = value;
        }
    }
}