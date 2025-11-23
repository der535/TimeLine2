using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace TimeLine
{
    [RequireComponent(typeof(TMP_InputField))]
    public class FloatInputValidator
    {
        private TMP_InputField _inputField;
        private Action<float> _onValidValueChanged;
        private Action<float> _onEndEdit;
        private readonly float _minValue;
        private readonly bool _hasMinValue;

        public FloatInputValidator(TMP_InputField inputField, Action<float> onValidValueChanged, Action<float> onEndEdit = null, float? minValue = null)
        {
            _onValidValueChanged = onValidValueChanged;
            _onEndEdit = onEndEdit;
            _inputField = inputField;
            _hasMinValue = minValue.HasValue;
            _minValue = minValue ?? 0f;

            _inputField.onValueChanged.AddListener(OnInputValueChanged);
            _inputField.onEndEdit.AddListener(OnInputEndEdit);
        }

        private void OnInputValueChanged(string input)
        {
            if (string.IsNullOrEmpty(input)) return;
            if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
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
                float value = _hasMinValue ? _minValue : 0f;
                _onEndEdit?.Invoke(value);
                _inputField.text = value.ToString(CultureInfo.InvariantCulture);
            }
            else if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed))
            {
                float clamped = _hasMinValue ? Mathf.Max(parsed, _minValue) : parsed;
                _onEndEdit?.Invoke(clamped);
                _inputField.text = clamped.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                // Некорректный ввод — сброс к минимуму или нулю
                float fallback = _hasMinValue ? _minValue : 0f;
                _onEndEdit?.Invoke(fallback);
                _inputField.text = fallback.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}