using System;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

namespace TimeLine.Keyframe
{
    using UnityEngine;

    public abstract class AnimationData : IAnimationApplyer
    {
        public abstract void Interpolate(AnimationData other, double t, Keyframe current, Keyframe next, Keyframe.InterpolationType interpolationType, Component target);
        public abstract void Apply(Component target, object value);
        public abstract Type GetComponentType();
        public abstract float4 PackDataToFloat4();
        
        // Добавленный абстрактный метод для клонирования
        public abstract AnimationData Clone();
        public abstract object GetValue();
        public abstract void SetValue(object value);
        // public abstract float4 PackDataToFloat4();
        
        public abstract string GetDataType();
        public virtual JObject SerializeData()
        {
            // По умолчанию сериализуем через рефлексию (для [Serializable] классов)
            return JObject.FromObject(this);
        }
        public abstract void DeserializeData(JObject data);
    
        // Для кастомных параметров
        public abstract void Apply(Component target, float4 value);
    }
}