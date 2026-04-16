using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.ValueEditor;
using TimeLine.LevelEditor.ValueEditor.NodeLogic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe
{
    public abstract class EntityAnimationData : IAnimationApplyer
    {
        public OutputLogic Logic;
        public List<IInitializedNode> initializedNodes = new();
        public GraphSaveData Graph;

        public abstract void Interpolate(EntityAnimationData other, double t,
            global::TimeLine.Keyframe.Keyframe current, global::TimeLine.Keyframe.Keyframe next,
            global::TimeLine.Keyframe.Keyframe.InterpolationType interpolationType, Entity target);

        public abstract void Apply(Entity target, object value);
        public abstract ComponentNames GetComponentType();
        public abstract float4 PackDataToFloat4();
        public abstract EntityAnimationData Clone();
        public abstract object GetValue();

        /// <summary>
        /// Получаем простые типы типа float или color
        /// </summary>
        /// <returns></returns>
        public abstract DataType? GetValueType(JObject data);

        public abstract void SetValue(object value);

        /// <summary>
        /// Получаем классы типа EntitySpriteRendererColor или EntityXPositionData
        /// </summary>
        /// <returns></returns>
        public abstract string GetDataType();

        public virtual JObject SerializeData()
        {
            // По умолчанию сериализуем через рефлексию (для [Serializable] классов)
            return JObject.FromObject(this);
        }

        public abstract void DeserializeData(JObject data);

        // Для кастомных параметров
        public abstract void Apply(Entity target, float4 value);
    }
}