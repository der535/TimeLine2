using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace TimeLine
{
    [RequireComponent(typeof(TMP_InputField))]
    public class StringInputValidator
    {
        private TMP_InputField _inputField;
        private Action<string> _onValidValueChanged;

        public StringInputValidator(TMP_InputField inputField, Action<string> onValidValueChanged)
        {
            _inputField = inputField;
            _onValidValueChanged = onValidValueChanged;

            _inputField.onValueChanged.AddListener(OnInputValueChanged);
            _inputField.onEndEdit.AddListener(OnInputEndEdit);
        }

        private void OnInputValueChanged(string input)
        {
            string trimmed = input.Trim();
            if (!string.IsNullOrEmpty(trimmed) && trimmed.All(c => !char.IsControl(c)))
            {
                _onValidValueChanged?.Invoke(trimmed);
            }
        }

        private void OnInputEndEdit(string input)
        {
            string trimmed = input.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.Any(char.IsControl))
            {
                // Сброс: можно оставить пустым или установить значение по умолчанию
                _onValidValueChanged?.Invoke(string.Empty);
                _inputField.text = string.Empty;
            }
            else
            {
                _onValidValueChanged?.Invoke(trimmed);
                _inputField.text = trimmed; // Применяем очистку на выходе
            }
        }
        
        public void Dispose()
        {
            if (_inputField != null)
            {
                _inputField.onValueChanged.RemoveListener(OnInputValueChanged);
                _inputField.onEndEdit.RemoveListener(OnInputEndEdit);
            }
        }
    }
}