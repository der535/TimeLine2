using System.Collections.Generic;

namespace TimeLine.CustomInspector.Logic.Parameter
{
    public class DropDownParameter : InspectableParameter
    {
        private List<string> _options;
        public List<string> Options
        {
            get => _options;
            set
            {
                if (_options == value) return;
                _options = value;
                NotifyValueChanged();
            }
        }

        private int _value;
        public int Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                NotifyValueChanged();
            }
        }

        // Исправленный конструктор
        public DropDownParameter(string name, List<string> options, int initialValue)
            : base(name, typeof(int)) // Передаем правильный тип значения
        {
            _options = options;
            _value = initialValue;
        }
    }
}