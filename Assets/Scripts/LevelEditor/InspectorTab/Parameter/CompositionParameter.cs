using System;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using UnityEngine;

namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class CompositionParameter : InspectableParameter
    {
        private GroupGameObjectSaveData _value;
        public GroupGameObjectSaveData Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                NotifyValueChanged();
            }
        }

        public CompositionParameter(string name, GroupGameObjectSaveData initialValue, Color animationColor) 
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
                Value = (GroupGameObjectSaveData)value;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to convert {value?.GetType()} to int: {ex.Message}");
            }
        }
    }
}