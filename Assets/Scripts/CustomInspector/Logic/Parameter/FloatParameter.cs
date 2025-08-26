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

    public FloatParameter(string name, float initialValue) 
        : base(name, typeof(float))
    {
        _value = initialValue;
    }
}