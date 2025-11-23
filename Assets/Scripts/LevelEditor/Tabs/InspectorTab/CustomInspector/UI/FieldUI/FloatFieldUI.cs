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
        
        private FloatInputValidator _inputValidator;
        private FloatParameter _floatParameter;

        public void Setup(FloatParameter floatParameter, Action createKeyframe)
        {
            _floatParameter = floatParameter;
            parameterName.text = floatParameter.Name;
            inputField.text = floatParameter.Value.ToString(CultureInfo.InvariantCulture);

            _inputValidator = new FloatInputValidator(inputField, value => _floatParameter.Value = value, value => _floatParameter.Value = value);

            // floatParameter.OnValueChanged += () =>
            //     inputField.text = _floatParameter.Value.ToString(CultureInfo.InvariantCulture);

            createKeyframeButton.onClick.AddListener(() => createKeyframe());
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }
    }
}