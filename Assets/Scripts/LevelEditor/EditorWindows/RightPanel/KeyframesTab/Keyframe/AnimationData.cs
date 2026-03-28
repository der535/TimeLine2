using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TimeLine.LevelEditor.ValueEditor;
using TimeLine.LevelEditor.ValueEditor.NodeLogic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe
{
    public abstract class AnimationData : IAnimationApplyer
    {
        public OutputLogic Logic;
        public List<IInitializedNode> initializedNodes = new();
        public string Graph;

        public abstract void Interpolate(AnimationData other, double t, global::TimeLine.Keyframe.Keyframe current,
            global::TimeLine.Keyframe.Keyframe next,
            global::TimeLine.Keyframe.Keyframe.InterpolationType interpolationType, Component target);

        public abstract Type GetComponentType();
        public abstract float4 PackDataToFloat4();
        public abstract AnimationData Clone();
        public abstract object GetValue();
        public abstract void SetValue(object value);
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