using Newtonsoft.Json.Linq;
using TimeLine.TimeLine;

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
        
        public override string GetDataType()
        {
            return nameof(YScaleData);
        }

        public override JObject SerializeData()
        {
            return new JObject
            {
                ["transform-scale-y"] = JToken.FromObject(value)
            };
        }

        public override void DeserializeData(JObject data)
        {
            if (data.TryGetValue("transform-scale-y", out JToken token))
            {
                value = token.ToObject<float>();
            }
        }
        
        public override AnimationData Interpolate(AnimationData other, double t, global::TimeLine.Keyframe.Keyframe current, global::TimeLine.Keyframe.Keyframe next, global::TimeLine.Keyframe.Keyframe.InterpolationType interpolationType)
        {
            if (other is not YScaleData otherPos)
                throw new System.ArgumentException("Interpolation requires another XPositionData.");

            float localT = (float)t;
            float interpolatedValue = TimeLineConverter.Instance.Interpolate(
                value,
                otherPos.value,
                current,
                next,
                localT,
                interpolationType
            );

            return new YScaleData(interpolatedValue);
        }

        public override void Apply(GameObject target)
        {
            global::TimeLine.TransformComponent transformComponent = target.GetComponent<global::TimeLine.TransformComponent>();
            transformComponent.YScale.Value = value;
        }
    }
}