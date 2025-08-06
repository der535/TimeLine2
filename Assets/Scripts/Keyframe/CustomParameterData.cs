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
            CustomParameterData otherData = (CustomParameterData)other;
            
            if (valueType == typeof(float))
            {
                float a = (float)Value;
                float b = (float)otherData.Value;
                return new CustomParameterData(ParameterName, Mathf.Lerp(a, b, t));
            }
            else if (valueType == typeof(Vector2))
            {
                Vector2 a = (Vector2)Value;
                Vector2 b = (Vector2)otherData.Value;
                return new CustomParameterData(ParameterName, Vector2.Lerp(a, b, t));
            }
            // Добавьте другие типы по необходимости
            
            return this; // Если тип не поддерживает интерполяцию
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