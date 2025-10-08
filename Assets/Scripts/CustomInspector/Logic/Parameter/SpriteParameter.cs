using UnityEngine;

namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class SpriteParameter : InspectableParameter
    {
        private Sprite _value;
        public Sprite Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                NotifyValueChanged();
            }
        }

        public SpriteParameter(string name, Sprite initialValue, Color animationColor) 
            : base(name, typeof(Sprite))
        {
            _value = initialValue;
            AnimationColor = animationColor;
        }
        public override object GetValue() => _value;
        public override void SetValue(object value)
        {
            if (value is Sprite spriteValue)
            {
                Value = spriteValue; // используем свойство, чтобы триггернуть OnValueChanged
            }
            else
            {
                Debug.LogWarning($"Cannot assign {value?.GetType()} to {_value.GetType().Name}");
            }
        }
    }
}