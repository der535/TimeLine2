using UnityEngine;

namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class FloatParameter : InspectableParameter
    {
        private float _value;
        public float Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                NotifyValueChanged();
            }
        }

        public FloatParameter(string name, float initialValue, Color animationColor) 
            : base(name, typeof(float))
        {
            _value = initialValue;
            AnimationColor = animationColor;
        }
        public Color AnimationColor { get; set; }
        public override object GetValue() => _value;
        public override void SetValue(object value)
        {
            if (value is float floatValue)
            {
                Value = floatValue; // используем свойство, чтобы триггернуть OnValueChanged
            }
            else
            {
                Debug.LogWarning($"Cannot assign {value?.GetType()} to {_value.GetType().Name}");
            }
        }
    }
}