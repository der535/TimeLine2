namespace TimeLine.Keyframe
{
    using UnityEngine;
    
    [System.Serializable]
    public class CustomParameterData : AnimationData
    {
        public string ParameterName { get; }
        public object Value { get; private set; }
        private System.Type valueType;

        public CustomParameterData(string name, object value)
        {
            ParameterName = name;
            Value = value;
            valueType = value.GetType();
        }

        public override AnimationData Interpolate(AnimationData other, float t)
        {
            if (!(other is CustomParameterData otherData)) return this;

            if (Value is float floatVal && otherData.Value is float otherFloat)
                return new CustomParameterData(ParameterName, Mathf.Lerp(floatVal, otherFloat, t));
    
            if (Value is Vector2 vec2Val && otherData.Value is Vector2 otherVec2)
                return new CustomParameterData(ParameterName, Vector2.Lerp(vec2Val, otherVec2, t));
    
            if (Value is int intVal && otherData.Value is int otherInt)
                return new CustomParameterData(ParameterName, (int)Mathf.Lerp(intVal, otherInt, t));
    
            return this; // Без интерполяции
        }

        public override void Apply(GameObject target)
        {
            if (target == null) return;
    
            var provider = target.GetComponent<IFieldProvider>();
            if (provider != null)
            {
                foreach (var field in provider.GetFields())
                {
                    if (field.Name == ParameterName)
                    {
                        field.ValueAsObject = Value;
                        break;
                    }
                }
            }
        }
    }
}