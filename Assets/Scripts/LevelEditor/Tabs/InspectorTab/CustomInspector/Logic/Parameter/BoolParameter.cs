using UnityEngine;

namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class BoolParameter : InspectableParameter
    {
        private bool _value;
        public bool Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                NotifyValueChanged();
            }
        }

        public BoolParameter(string name, bool initialValue, Color animationColor) 
            : base(name, typeof(bool))
        {
            _value = initialValue;
            AnimationColor = animationColor;
        }

        public override object GetValue() => _value;

        public override void SetValue(object value)
        {
            if (value is bool boolValue)
            {
                Value = boolValue; // используем свойство, чтобы триггернуть OnValueChanged
            }
            else
            {
                Debug.LogWarning($"Cannot assign {value?.GetType()} to BoolParameter");
            }
        }
    }
}