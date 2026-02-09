using System;
using Newtonsoft.Json.Linq;
using TimeLine.TimeLine;
using Unity.Mathematics;
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

        public override Type GetComponentType()
        {
            return typeof(global::TimeLine.TransformComponent);
        }

        public override float4 PackDataToFloat4()
        {
            return new float4(value, 0,0,0);
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
            if (value is float f) this.value = f;
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

        public override void Apply(Component target, float4 value)
        {
            if (target is global::TimeLine.TransformComponent component)
            {
                component.YPosition.Value = value.x;
            }
        }

        public override void Interpolate(AnimationData other, double t, Keyframe current, Keyframe next,
            Keyframe.InterpolationType interpolationType, Component target)
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

            Apply(target, interpolatedValue);
        }

        public override void Apply(Component target, object o)
        {
            if (target is global::TimeLine.TransformComponent component)
            {
                component.YPosition.Value = (float)o;
            }
        }
    }
}