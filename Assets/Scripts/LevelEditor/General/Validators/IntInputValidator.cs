using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace TimeLine
{
    [RequireComponent(typeof(TMP_InputField))]
    public class IntInputValidator
    {
        private TMP_InputField _inputField;
        private Action<int> _onValidValueChanged;
        private Action<int> _onEndEdit;
        private readonly int _minValue;
        private readonly bool _hasMinValue;

        public IntInputValidator(TMP_InputField inputField, Action<int> onValidValueChanged, Action<int> onEndEdit = null, int? minValue = null)
        {
            _onValidValueChanged = onValidValueChanged;
            _onEndEdit = onEndEdit;
            _inputField = inputField;
            _hasMinValue = minValue.HasValue;
            _minValue = minValue ?? 0;

            _inputField.onValueChanged.AddListener(OnInputValueChanged);
            _inputField.onEndEdit.AddListener(OnInputEndEdit);
        }

        private void OnInputValueChanged(string input)
        {
            if (string.IsNullOrEmpty(input)) return;
            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                if (_hasMinValue && result < _minValue)
                    result = _minValue;

                _onValidValueChanged?.Invoke(result);
            }
        }

        private void OnInputEndEdit(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                int value = _hasMinValue ? _minValue : 0;
                _onEndEdit?.Invoke(value);
                _inputField.text = value.ToString(CultureInfo.InvariantCulture);
            }
            else if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
            {
                int clamped = _hasMinValue ? Mathf.Max(parsed, _minValue) : parsed;
                _onEndEdit?.Invoke(clamped);
                _inputField.text = clamped.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                int fallback = _hasMinValue ? _minValue : 0;
                _onEndEdit?.Invoke(fallback);
                _inputField.text = fallback.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}