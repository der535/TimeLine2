using System;
using Newtonsoft.Json.Linq;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
using TimeLine.TimeLine;
using Unity.Mathematics;
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

        public override float4 PackDataToFloat4()
        {
            return new float4(value,0,0,0);
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

        public override void Apply(Component target, float4 value)
        {
            if(target is global::TimeLine.TransformComponent component)
                component.XScale.Value = value.x;
        }

        public override void Interpolate(AnimationData other, double t, global::TimeLine.Keyframe.Keyframe current, global::TimeLine.Keyframe.Keyframe next, global::TimeLine.Keyframe.Keyframe.InterpolationType interpolationType, Component target)
        {
            if (other is not XScaleData otherPos)
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

        public override Type GetComponentType()
        {
            return typeof(global::TimeLine.TransformComponent);
        }
        public override void Apply(Component target, object o)
        {
            if(target is global::TimeLine.TransformComponent component)
                component.XScale.Value = (float)o;
        }
    }
}