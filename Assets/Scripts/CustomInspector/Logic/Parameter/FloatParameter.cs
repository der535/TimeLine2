using UnityEngine;

namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class FloatParameter : InspectableParameter, IParameterColor
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
    }
}