using System;
using TimeLine.LevelEditor.GeneralServices;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
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
            Id = UniqueIDGenerator.GenerateUniqueID();
        }
        public override object GetValue() => _value;
        public override void SetValue(object value)
        {
            try
            {
                Value = value == null ? 0 : Convert.ToSingle(value);
            }
            catch
            {
                Debug.LogWarning($"Failed to convert {value?.GetType()} to float");
            }
        }
    }
}