using System;
using Newtonsoft.Json.Linq;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.ValueEditor;
using TimeLine.TimeLine;
using Unity.Mathematics;
using UnityEngine;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Position
{
    [System.Serializable]
    public class XPositionData : AnimationData
    {
        public float value;
        public OutputLogic logic;
        public string SaveGraph;
        

        public XPositionData(float value)
        {
            this.value = value;
            logic = new OutputLogic();
            logic.Initialize(DataType.Float);
            logic.ManualValues[0] = value;
        }

        public override Type GetComponentType()
        {
            return typeof(global::TimeLine.TransformComponent);
        }

        public override float4 PackDataToFloat4()
        {
            return new float4(value,0,0,0);
        }

        public override AnimationData Clone()
        {
            return new XPositionData(value);
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
           return nameof(XPositionData);
        }

        public override JObject SerializeData()
        {
            return new JObject
            {
                ["transform-position-x"] = JToken.FromObject(value)
            };
        }

        public override void DeserializeData(JObject data)
        {
            if (data.TryGetValue("transform-position-x", out JToken token))
            {
                value = token.ToObject<float>();
            }
        }

        public override void Apply(Component target, float4 value)
        {
            if (target is global::TimeLine.TransformComponent component)
            {
                component.XPosition.Value = value.x;
            }
        }

        public override void Interpolate(
            AnimationData other, 
            double t, 
            global::TimeLine.Keyframe.Keyframe current, 
            global::TimeLine.Keyframe.Keyframe next,
            global::TimeLine.Keyframe.Keyframe.InterpolationType interpolationType, Component target)
        {
            if (other is not XPositionData otherPos)
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
                component.XPosition.Value = (float)o;
            }
        }
    }
}