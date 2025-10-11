using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class ColorParameter : InspectableParameter
    {
        private Color _value;
        public Color Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                NotifyValueChanged();
            }
        }

        public ColorParameter(string name, Color initialValue, Color animationColor) 
            : base(name, typeof(Color))
        {
            _value = initialValue;
            AnimationColor = animationColor;
        }
        public Color AnimationColor { get; set; }
        public override object GetValue() => _value;
        public override void SetValue(object value)
        {
            if (value is Color color)
            {
                Value = color;
            }
            else if (value is JObject jObject)
            {
                try
                {
                    float r = jObject["r"]?.ToObject<float>() ?? 0f;
                    float g = jObject["g"]?.ToObject<float>() ?? 0f;
                    float b = jObject["b"]?.ToObject<float>() ?? 0f;
                    float a = jObject["a"]?.ToObject<float>() ?? 1f;
                    Value = new Color(r, g, b, a);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to parse Color from JObject: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Cannot assign {value?.GetType()} to ColorParameter");
            }
        }
    }
}