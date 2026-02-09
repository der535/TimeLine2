using System;
using Newtonsoft.Json.Linq;
using TimeLine.TimeLine;
using Unity.Mathematics;
using UnityEngine;

namespace TimeLine.Keyframe.AnimationDatas.BoxCollider.Scale
{
    [System.Serializable]
    public class YSizeData : AnimationData
    {
        public float value;

        public YSizeData(float value)
        {
            this.value = value;
        }

        public override Type GetComponentType()
        {
           return typeof(BoxCollider2DComponent);
        }

        public override float4 PackDataToFloat4()
        {
            return new float4(value, 0, 0, 0);
        }

        public override AnimationData Clone()
        {
            return new YSizeData(value);
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
           return nameof(YSizeData);
        }

        public override JObject SerializeData()
        {
            return new JObject
            {
                ["boxCollider-offset-x"] = JToken.FromObject(value)
            };
        }

        public override void DeserializeData(JObject data)
        {
            if (data.TryGetValue("boxCollider-offset-x", out JToken token))
            {
                value = token.ToObject<float>();
            }
        }

        public override void Apply(Component target, float4 value)
        {
            if (target is BoxCollider2DComponent component)
            {
                component.SizeY.Value = value.x;
            }
        }

        public override void Interpolate(
            AnimationData other, 
            double t, 
            global::TimeLine.Keyframe.Keyframe current, 
            global::TimeLine.Keyframe.Keyframe next,
            Keyframe.InterpolationType interpolationType, Component target)
        {
            if (other is not YSizeData otherPos)
                throw new System.ArgumentException($"Interpolation requires another {GetType()}.");

            float localT = (float)t;
            float interpolatedValue = TimeLineConverter.Instance.Interpolate(
                value,
                otherPos.value,
                current,
                next,
                localT,
                interpolationType
            );
            
            Apply(target, value);
        }

        public override void Apply(Component target, object o)
        {
            if (target is BoxCollider2DComponent component)
            {
                component.SizeY.Value = (float)o;
            }
        }
    }
}