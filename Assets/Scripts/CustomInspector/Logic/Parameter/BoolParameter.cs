namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class BoolParameter : InspectableParameter
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

        public BoolParameter(string name, bool initialValue) 
            : base(name, typeof(bool))
        {
            _value = initialValue;
        }
    }
}