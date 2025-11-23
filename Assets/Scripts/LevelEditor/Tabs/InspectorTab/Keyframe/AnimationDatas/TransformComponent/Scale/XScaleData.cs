using Newtonsoft.Json.Linq;
using TimeLine.TimeLine;
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
            return nameof(XScaleData);
        }

        public override JObject SerializeData()
        {
            return new JObject
            {
                ["transform-scale-x"] = JToken.FromObject(value)
            };
        }

        public override void DeserializeData(JObject data)
        {
            if (data.TryGetValue("transform-scale-x", out JToken token))
            {
                value = token.ToObject<float>();
            }
        }
        
        public override AnimationData Interpolate(AnimationData other, double t, global::TimeLine.Keyframe.Keyframe current, global::TimeLine.Keyframe.Keyframe next)
        {
            if (other is not XScaleData otherPos)
                throw new System.ArgumentException("Interpolation requires another XPositionData.");

            float localT = (float)t;
            float interpolatedValue = TimeLineConverter.Instance.Interpolate(
                value,
                otherPos.value,
                current,
                next,
                localT
            );

            return new XScaleData(interpolatedValue);
        }

        public override void Apply(GameObject target)
        {
            global::TimeLine.TransformComponent transformComponent = target.GetComponent<global::TimeLine.TransformComponent>();
            transformComponent.XScale.Value = value;
        }
    }
}