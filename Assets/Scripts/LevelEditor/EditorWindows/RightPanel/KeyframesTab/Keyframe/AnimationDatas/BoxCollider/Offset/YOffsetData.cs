using System;
using Newtonsoft.Json.Linq;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
using TimeLine.TimeLine;
using Unity.Mathematics;
using UnityEngine;

namespace TimeLine.Keyframe.AnimationDatas.BoxCollider.Offset
{
    [System.Serializable]
    public class YOffsetData : AnimationData
    {
        public float value;

        public YOffsetData(float value)
        {
            this.value = value;
        }

        public override Type GetComponentType()
        {
            return typeof(BoxCollider2DComponent);
        }

        public override float4 PackDataToFloat4()
        {
            throw new NotImplementedException();
        }

        public override AnimationData Clone()
        {
            return new YOffsetData(value);
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
            return nameof(YOffsetData);
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
            throw new NotImplementedException();
        }

        public override void Interpolate(
            AnimationData other,
            double t,
            global::TimeLine.Keyframe.Keyframe current,
            global::TimeLine.Keyframe.Keyframe next,
            Keyframe.InterpolationType interpolationType, Component target)
        {
            if (other is not YOffsetData otherPos)
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

            Apply(target, interpolatedValue);
        }

        public override void Apply(Component target, object o)
        {
            if (target is BoxCollider2DComponent component)
            {
                component.OffsetY.Value = (float)o;
            }
        }
    }
}