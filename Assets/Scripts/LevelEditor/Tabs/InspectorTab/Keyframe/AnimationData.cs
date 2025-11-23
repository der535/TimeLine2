using Newtonsoft.Json.Linq;

namespace TimeLine.Keyframe
{
    using UnityEngine;

    public abstract class AnimationData
    {
        public abstract AnimationData Interpolate(AnimationData other, double t, Keyframe current, Keyframe next);
        public abstract void Apply(GameObject target);
        
        // Добавленный абстрактный метод для клонирования
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
        public virtual void ApplyToComponent(Component component) { }
    }
}