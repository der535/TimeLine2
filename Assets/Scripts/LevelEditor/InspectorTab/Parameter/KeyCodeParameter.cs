using System;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using UnityEngine;

namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class KeyCodeParameter : InspectableParameter
    {
        private KeyCode _value;
        public KeyCode Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                NotifyValueChanged();
            }
        }

        public KeyCodeParameter(string name, KeyCode initialValue, Color animationColor) 
            : base(name, typeof(float))
        {
            _value = initialValue;
            AnimationColor = animationColor;
        }
        public override object GetValue() => _value;
        public override void SetValue(object value)
        {
            try
            {
                Value = (KeyCode)value;
            }
            catch
            {
                Debug.LogWarning($"Failed to convert {value?.GetType()} to float");
            }
        }
    }
}