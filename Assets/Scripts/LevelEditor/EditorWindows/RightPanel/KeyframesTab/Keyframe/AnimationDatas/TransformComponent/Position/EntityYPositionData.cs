using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.ValueEditor;
using TimeLine.LevelEditor.ValueEditor.Test;
using TimeLine.TimeLine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Position
{
    [System.Serializable]
    public class EntityXPositionData : EntityAnimationData
    {
        public EntityXPositionData(float value)
        {
            Logic = new OutputLogic();
            Logic.Initialize(DataType.Float);
            Logic.ManualValues[0] = value;
            Graph = SaveGraph.ToJson(Logic);
        }
        
        public override Type GetComponentType()
        {
            return typeof(global::TimeLine.TransformComponent);
        }

        public override float4 PackDataToFloat4()
        {
            return new float4((float)Logic.GetValue(), 0, 0, 0);
        }

        public override AnimationData Clone()
        {
            return new XPositionData((float)Logic.GetValue());
        }

        public override object GetValue()
        {
            
            return (float)Logic.GetValue();
        }

        public override void SetValue(object value)
        {
            if (value is float f) Logic.ManualValues[0] = f;
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
                ["transform-position-x"] = JToken.FromObject((float)Logic.GetValue())
            };
        }

        public override void DeserializeData(JObject data)
        {
            if (data.TryGetValue("transform-position-x", out JToken token))
            {
                Logic.Initialize(DataType.Float);
                Logic.ManualValues[0] = token.ToObject<float>();
                Graph = SaveGraph.ToJson(Logic);
            }
        }

        public override void Apply(Entity target, float4 value)
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            LocalTransform localTransform = manager.GetComponentData<LocalTransform>(target);
            localTransform.Position.x = value.x;
            manager.SetComponentData(target, localTransform);
        }

        public override void Interpolate(
            EntityAnimationData other,
            double t,
            global::TimeLine.Keyframe.Keyframe current,
            global::TimeLine.Keyframe.Keyframe next,
            global::TimeLine.Keyframe.Keyframe.InterpolationType interpolationType, Entity target)
        {
            if (other is not EntityXPositionData otherPos)
                throw new System.ArgumentException("Interpolation requires another XPositionData.");

            float localT = (float)t;
            float interpolatedValue = TimeLineConverter.Instance.Interpolate(
                (float)Logic.GetValue(),
                (float)otherPos.Logic.GetValue(),
                current,
                next,
                localT,
                interpolationType
            );

            Apply(target, interpolatedValue);
        }

        public override void Apply(Entity target, object o)
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            LocalTransform localTransform = manager.GetComponentData<LocalTransform>(target);
            localTransform.Position.x = (float)o;
            manager.SetComponentData(target, localTransform);
        }
    }
}