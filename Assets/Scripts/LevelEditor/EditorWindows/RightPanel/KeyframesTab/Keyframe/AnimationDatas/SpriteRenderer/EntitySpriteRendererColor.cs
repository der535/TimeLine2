using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using TimeLine.LevelEditor.ValueEditor;
using TimeLine.LevelEditor.ValueEditor.Test;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Position
{
    [System.Serializable]
    public class EntitySpriteRendererColor : EntityAnimationData
    {
        public EntitySpriteRendererColor(Color value)
        {
            Logic = new OutputLogic();
            Logic.Initialize(DataType.Color);
            Logic.ManualValues[0] = value;
            Graph = SaveGraph.ToJson(Logic, DataType.Color);
        }

        public override ComponentNames GetComponentType()
        {
            return ComponentNames.SpriteRenderer;
        }

        public override float4 PackDataToFloat4()
        {
            Color color = (Color)Logic.GetValue();
            return new float4(color.r, color.g, color.b, color.a);
        }

        public override EntityAnimationData Clone()
        {
            return new EntitySpriteRendererColor((Color)Logic.GetValue());
        }

        public override object GetValue()
        {
            return (Color)Logic.GetValue();
        }

        public override DataType? GetValueType(JObject data)
        {
            if (data.TryGetValue("spriterenderer-color", out JToken token))
            {
                return DataType.Color;
            }

            return null;
        }

        public override void SetValue(object value)
        {
            if (value is Color f) Logic.ManualValues[0] = f;
            else
            {
                Debug.LogWarning("[TimeLine.Keyframe] Cannot set XPositionData value to a float");
            }
        }

        public override string GetDataType()
        {
            return nameof(EntitySpriteRendererColor);
        }

        public override JObject SerializeData()
        {
            return new JObject
            {
                ["spriterenderer-color"] = JToken.FromObject((Color)Logic.GetValue())
            };
        }
        
        

        public override void DeserializeData(JObject data)
        {
            if (data.TryGetValue("spriterenderer-color", out JToken token))
            {
                Logic.Initialize(DataType.Color);
                Logic.ManualValues[0] = token.ToObject<Color>();
                Graph = SaveGraph.ToJson(Logic, DataType.Color);
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
            if (other is not EntitySpriteRendererColor otherPos)
                throw new ArgumentException("Interpolation requires another XPositionData.");

            float localT = (float)t;
            
            Color color1 = (Color)Logic.GetValue();
            Color color2 = (Color)otherPos.Logic.GetValue();
            
            float interpolatedR = TimeLineConverter.Instance.Interpolate(
                color1.r,
                color2.r,
                current,
                next,
                localT,
                interpolationType
            );
            
            float interpolatedG = TimeLineConverter.Instance.Interpolate(
                color1.g,
                color2.g,
                current,
                next,
                localT,
                interpolationType
            );
            
            float interpolatedB = TimeLineConverter.Instance.Interpolate(
                color1.b,
                color2.b,
                current,
                next,
                localT,
                interpolationType
            );
            
            float interpolatedA = TimeLineConverter.Instance.Interpolate(
                color1.a,
                color2.a,
                current,
                next,
                localT,
                interpolationType
            );

            Apply(target, new Color(interpolatedR, interpolatedG, interpolatedB, interpolatedA));
        }

        public override void Apply(Entity target, object o)
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Material currentMat = null;  
            RenderMeshArray rma = manager.GetSharedComponentManaged<RenderMeshArray>(target);  
            if (manager.HasComponent<MaterialMeshInfo>(target))  
            {  
                var meshInfo = manager.GetComponentData<MaterialMeshInfo>(target);  
  
                // Получаем текущий материал  
                currentMat = rma.GetMaterial(meshInfo);  
            }
            currentMat.color = (Color)o;
        }
    }
}