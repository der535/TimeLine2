using System;
using System.Collections.Generic;
using System.Linq;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using UnityEngine;

namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class ListVector2Parameter : InspectableParameter
    {
        private List<Vector2> _value;
        public List<Vector2> Value
        {
            get => _value.ToList();
            set
            {
                if (_value == value) return;
                _value = value;
                NotifyValueChanged();
            }
        }

        public ListVector2Parameter(string name, List<Vector2> initialValue, Color animationColor) 
            : base(name, typeof(List<Vector2>))
        {
            _value = initialValue;
            AnimationColor = animationColor;
        }
        public override object GetValue() => _value;
        public override void SetValue(object value)
        {
            try
            {
                Debug.Log(value);
                Value = value == null ? new List<Vector2>() : (List<Vector2>)value;
            }
            catch
            {
                Debug.LogWarning($"Failed to convert {value?.GetType()} to {_value.GetType()}");
            }
        }
    }
}