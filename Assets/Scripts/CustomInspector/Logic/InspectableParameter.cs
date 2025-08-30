namespace TimeLine.CustomInspector.Logic
{
    public abstract class InspectableParameter
    {
        public string Name { get; }
        public System.Type ValueType { get; }
    
        public event System.Action OnValueChanged;

        protected InspectableParameter(string name, System.Type valueType)
        {
            Name = name;
            ValueType = valueType;
        }

        public void NotifyValueChanged() => OnValueChanged?.Invoke();
    }
}