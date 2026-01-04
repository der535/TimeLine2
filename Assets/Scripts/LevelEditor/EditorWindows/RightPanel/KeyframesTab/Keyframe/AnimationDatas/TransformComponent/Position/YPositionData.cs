using Newtonsoft.Json.Linq;
using TimeLine.TimeLine;
using UnityEngine;

namespace TimeLine.Keyframe.AnimationDatas.TransformComponent.Position
{
    [System.Serializable]
    public class YPositionData : AnimationData
    {
        public float value;

        public YPositionData(float value)
        {
            this.value = value;
        }

        public override AnimationData Clone()
        {
            return new YPositionData(value);
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
            return nameof(YPositionData);
        }

        public override JObject SerializeData()
        {
            return new JObject
            {
                ["transform-position-y"] = JToken.FromObject(value)
            };
        }

        public override void DeserializeData(JObject data)
        {
            if (data.TryGetValue("transform-position-y", out JToken token))
            {
                value = token.ToObject<float>();
            }
        }

        public override AnimationData Interpolate(AnimationData other, double t, Keyframe current, Keyframe next, Keyframe.InterpolationType interpolationType)
        {
            if (other is not YPositionData otherPos)
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

            return new YPositionData(interpolatedValue);
        }

        public override void Apply(GameObject target)
        {
            global::TimeLine.TransformComponent transformComponent = target.GetComponent<global::TimeLine.TransformComponent>();
            transformComponent.YPosition.Value = value;
        }
    }
}