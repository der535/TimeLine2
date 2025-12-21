using System;
using UnityEngine;

namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class IntParameter : InspectableParameter
    {
        private int _value;
        public int Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                NotifyValueChanged();
            }
        }

        public IntParameter(string name, int initialValue, Color animationColor) 
            : base(name, typeof(int))
        {
            _value = initialValue;
            AnimationColor = animationColor;
        }
        public override object GetValue() => _value;
        public override void SetValue(object value)
        {
            try
            {
                Value = value == null ? 0 : Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to convert {value?.GetType()} to int: {ex.Message}");
            }
        }
    }
}