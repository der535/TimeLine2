using System;
using Newtonsoft.Json.Linq;
using TimeLine.Keyframe;
using TimeLine.TimeLine;
using Unity.Mathematics;
using UnityEngine;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.RadialSunburstDrawer
{
    [System.Serializable]
    public class RadialSunburstMaterialColor2 : AnimationData
    {
        public Color value;

        public RadialSunburstMaterialColor2(Color value)
        {
            this.value = value;
        }

        public override Type GetComponentType()
        {
            return typeof(RadialSunburstMaterial);
        }

        public override float4 PackDataToFloat4()
        {
           return new float4(value.r, value.g, value.b, value.a);
        }

        public override AnimationData Clone()
        {
            return new RadialSunburstMaterialColor2(value);
        }

        public override object GetValue()
        {
            return value;
        }

        public override void SetValue(object value)
        {
            if(value is Color f) this.value = f;
            else
            {
                Debug.LogWarning("[TimeLine.Keyframe] Cannot set XPositionData value to a float");
            }
        }

        public override string GetDataType()
        {
           return nameof(RadialSunburstMaterial);
        }

        public override JObject SerializeData()
        {
            return new JObject
            {
                ["color2"] = JToken.FromObject(value)
            };
        }

        public override void DeserializeData(JObject data)
        {
            if (data.TryGetValue("color2", out JToken token))
            {
                value = token.ToObject<Color>();
            }
        }

        public override void Apply(Component target, float4 value)
        {
            if(target is RadialSunburstMaterial component)
            {
                component.Color2.Value = new Color(value.x, value.y, value.z, value.w);
            }
        }

        public override void Interpolate(
            AnimationData other, 
            double t, 
            global::TimeLine.Keyframe.Keyframe current, 
            global::TimeLine.Keyframe.Keyframe next,
            global::TimeLine.Keyframe.Keyframe.InterpolationType interpolationType, Component target)
        {
            if (other is not RadialSunburstMaterialColor2 otherPos)
                throw new System.ArgumentException($"Interpolation requires another {GetType()}.");

            float localT = (float)t;
            
            float interpolatedAlpha = TimeLineConverter.Instance.Interpolate(
                value.a,
                otherPos.value.a,
                current,
                next,
                localT,
                interpolationType
            );
            float interpolatedGreen = TimeLineConverter.Instance.Interpolate(
                value.g,
                otherPos.value.g,
                current,
                next,
                localT,
                interpolationType
            );
            float interpolatedBlue = TimeLineConverter.Instance.Interpolate(
                value.b,
                otherPos.value.b,
                current,
                next,
                localT,
                interpolationType
            );
            float interpolatedRed = TimeLineConverter.Instance.Interpolate(
                value.r,
                otherPos.value.r,
                current,
                next,
                localT,
                interpolationType
            );
            
            Color result = new Color(interpolatedRed, interpolatedGreen, interpolatedBlue, interpolatedAlpha);

            Apply(target, result);
        }

        public override void Apply(Component target, object o)
        {
            if(target is RadialSunburstMaterial component)
            {
                component.Color2.Value = value;
            }
        }
    }
}