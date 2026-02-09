using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using UnityEngine;

namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class StringParameter : InspectableParameter
    {
        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                NotifyValueChanged();
            }
        }

        public StringParameter(string name, string initialValue) 
            : base(name, typeof(string))
        {
            _value = initialValue;
        }
        public override object GetValue() => _value;
        public override void SetValue(object value)
        {
            if (value is string stringValue)
            {
                Value = stringValue; // используем свойство, чтобы триггернуть OnValueChanged
            }
            else
            {
                Debug.LogWarning($"Cannot assign {value?.GetType()} to {_value.GetType().Name}");
            }
        }
    }
}