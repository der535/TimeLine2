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
            if (value is Color colorValue)
            {
                Value = colorValue; // используем свойство, чтобы триггернуть OnValueChanged
            }
            else
            {
                Debug.LogWarning($"Cannot assign {value?.GetType()} to {_value.GetType().Name}");
            }
        }
    }
}