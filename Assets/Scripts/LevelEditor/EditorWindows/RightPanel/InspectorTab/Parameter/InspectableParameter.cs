using UnityEngine;

namespace TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic
{
    public abstract class InspectableParameter
    {
        public string Name { get; }
        public System.Type ValueType { get; }
        public Color AnimationColor { get; set; }
        public string Id  { get; set; }
        public event System.Action OnValueChanged;

        protected InspectableParameter(string name, System.Type valueType)
        {
            Name = name;
            ValueType = valueType;
        }

        public void NotifyValueChanged() => OnValueChanged?.Invoke();
        
        public abstract object GetValue();
        public abstract void SetValue(object value);
    }
}