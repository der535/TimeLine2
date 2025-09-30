using UnityEngine;

namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class BoolParameter : InspectableParameter, IParameterColor
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

        public Color AnimationColor { get; set; }
    }
}