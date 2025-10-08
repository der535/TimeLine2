using UnityEngine;

namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class Vector2Parameter : InspectableParameter
    {
        private Vector2 _value;
        private string _xName;
        private string _yName;
    
        public Vector2 Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                NotifyValueChanged();
            }
        }

        public string XName
        {
            get => _xName;
            set => _xName = value;
        }

        public string YName
        {
            get => _yName; 
            set => _yName = value; 
        }

        public Vector2Parameter(string name, string xName, string yName, Vector2 initialValue) 
            : base(name, typeof(Vector2))
        {
            _value = initialValue;
            _xName = xName;
            _yName = yName;
        }
        public override object GetValue() => _value;
        public override void SetValue(object value)
        {
            if (value is Vector2 vectorValue)
            {
                Value = vectorValue; // используем свойство, чтобы триггернуть OnValueChanged
            }
            else
            {
                Debug.LogWarning($"Cannot assign {value?.GetType()} to {_value.GetType().Name}");
            }
        }
    }
}