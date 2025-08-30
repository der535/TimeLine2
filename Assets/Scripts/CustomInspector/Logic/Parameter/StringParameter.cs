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
    }
}