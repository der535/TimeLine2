using System;
using System.Globalization;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.CustomInspector.UI.FieldUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class FloatFieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform fieldRect;
        [Space]
        [SerializeField] private TextMeshProUGUI parameterName;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button createKeyframeButton;

        private FloatParameter _floatParameter;

        public void Setup(FloatParameter floatParameter, Action createKeyframe)
        {
            _floatParameter = floatParameter;
            parameterName.text = floatParameter.Name;
            inputField.text = floatParameter.Value.ToString(CultureInfo.InvariantCulture);

            floatParameter.OnValueChanged += () =>
                inputField.text = floatParameter.Value.ToString(CultureInfo.InvariantCulture);

            inputField.onValueChanged.AddListener(OnInputValueChanged);
            inputField.onEndEdit.AddListener(OnInputEndEdit);
            createKeyframeButton.onClick.AddListener(() => createKeyframe());
        }

        private void OnInputValueChanged(string input)
        {
            if (string.IsNullOrEmpty(input)) return;
            if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                _floatParameter.Value = result;
            }
        }

        private void OnInputEndEdit(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                _floatParameter.Value = 0f;
                inputField.text = "0";
            }
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }
    }
}