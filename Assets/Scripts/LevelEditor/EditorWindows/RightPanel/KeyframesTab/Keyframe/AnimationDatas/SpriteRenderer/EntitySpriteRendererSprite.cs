using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.SpriteLoader;
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
    public class EntitySpriteRendererSprite : EntityAnimationData
    {
        public EntitySpriteRendererSprite(string spriteID)
        {
            Logic = new OutputLogic();
            Logic.Initialize(DataType.Sprite);
            Logic.ManualValues[0] = spriteID;
            Graph = SaveGraph.ToJson(Logic, DataType.Sprite);
        }

        public override ComponentNames GetComponentType()
        {
            return ComponentNames.SpriteRenderer;
        }

        public override float4 PackDataToFloat4()
        {
            return new float4(0);
        }

        public override EntityAnimationData Clone()
        {
            return new EntitySpriteRendererSprite((string)Logic.GetValue());
        }

        public override object GetValue()
        {
            return (string)Logic.GetValue();
        }

        public override DataType? GetValueType(JObject data)
        {
            if (data.TryGetValue("sprite-renderer-sprite", out JToken token))
            {
                return DataType.Sprite;
            }

            return null;
        }

        public override void SetValue(object value)
        {
            if (value is string f) Logic.ManualValues[0] = f;
            else
            {
                Debug.LogWarning("[TimeLine.Keyframe] Cannot set sprite value to a float");
            }
        }

        public override string GetDataType()
        {
            return nameof(EntitySpriteRendererSprite);
        }

        public override JObject SerializeData()
        {
            return new JObject
            {
                ["sprite-renderer-sprite"] = JToken.FromObject((string)Logic.GetValue())
            };
        }
        
        

        public override void DeserializeData(JObject data)
        {
            if (data.TryGetValue("sprite-renderer-sprite", out JToken token))
            {
                Logic.Initialize(DataType.Sprite);
                Logic.ManualValues[0] = token.ToObject<string>();
                Graph = SaveGraph.ToJson(Logic, DataType.Sprite);
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
            if (other is not EntitySpriteRendererSprite otherPos)
                throw new ArgumentException("Interpolation requires another XPositionData.");

            float localT = (float)t;
            
            string sprite1 = (string)Logic.GetValue();
            string sprite2 = (string)otherPos.Logic.GetValue();

            string currentSprite = sprite1;

            if (localT >= next.Ticks)
            {
                currentSprite = sprite2;
            }

            Apply(target, currentSprite);
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

            // Debug.Log((string)o);
            
            // Debug.Log(GetSpriteName.Instance.GetSpriteFromName((string)o).texture);
            currentMat.mainTexture = GetSpriteName.Instance.GetSpriteFromName((string)o).texture;
        }
    }
}