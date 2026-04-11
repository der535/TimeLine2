using System;
using Newtonsoft.Json.Linq;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.ECS;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.ValueEditor;
using TimeLine.LevelEditor.ValueEditor.Test;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Position
{
    [System.Serializable]
    public class EntityYRotationData : EntityAnimationData
    {
        public EntityYRotationData(float value)
        {
            Logic = new OutputLogic();
            Logic.Initialize(DataType.Float);
            Logic.ManualValues[0] = value;
            Graph = SaveGraph.ToJson(Logic, DataType.Float);
        }
        
        public override ComponentNames GetComponentType()
        {
            return ComponentNames.Transform;
        }

        public override float4 PackDataToFloat4()
        {
            return new float4((float)Logic.GetValue(), 0, 0, 0);
        }

        public override EntityAnimationData Clone()
        {
            return new EntityYRotationData((float)Logic.GetValue());
        }

        public override object GetValue()
        {
            
            return (float)Logic.GetValue();
        }

        public override DataType? GetValueType(JObject data)
        {
            if (data.TryGetValue("transform-position-y", out JToken token))
            {
                return DataType.Float;
            }

            return null;
        }

        public override void SetValue(object value)
        {
            if (value is float f) Logic.ManualValues[0] = f;
            else
            {
                Debug.LogWarning("[TimeLine.Keyframe] Cannot set yRotationData value to a float");
            }
        }

        public override string GetDataType()
        {
            return nameof(EntityYRotationData);
        }

        public override JObject SerializeData()
        {
            return new JObject
            {
                ["transform-position-y"] = JToken.FromObject((float)Logic.GetValue())
            };
        }

        public override void DeserializeData(JObject data)
        {
            if (data.TryGetValue("transform-position-y", out JToken token))
            {
                Logic.Initialize(DataType.Float);
                Logic.ManualValues[0] = token.ToObject<float>();
                Graph = SaveGraph.ToJson(Logic, DataType.Float);
            }
        }

        public override void Apply(Entity target, float4 value)
        {
            Apply(target, (object)value.x);
        }

        public override void Interpolate(
            EntityAnimationData other,
            double t,
            global::TimeLine.Keyframe.Keyframe current,
            global::TimeLine.Keyframe.Keyframe next,
            global::TimeLine.Keyframe.Keyframe.InterpolationType interpolationType, Entity target)
        {
            if (other is not EntityYRotationData otherPos)
                throw new System.ArgumentException("Interpolation requires another yRotationData.");

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
            
            Vector3 currentEuler = GetDegree.FromQuaternion(localTransform.Rotation);
            currentEuler.y = (float)o;
            localTransform.Rotation = GetDegree.FromEuler(currentEuler);
            
            manager.SetComponentData(target, localTransform);
        }
    }
}