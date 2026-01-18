using System;
using System.Globalization;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.CustomInspector.UI.FieldUI;
using TimeLine.LevelEditor.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TimeLine
{
    public class IntFieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform fieldRect;
        [Space]
        [SerializeField] private TextMeshProUGUI parameterName;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private EventTrigger createKeyframeButton;
        
        private IntInputValidator _inputValidator;
        private IntParameter _intParameter;

        public void Setup(IntParameter floatParameter, Action createKeyframe)
        {
            _intParameter = floatParameter;
            parameterName.text = floatParameter.Name;
            inputField.text = floatParameter.Value.ToString(CultureInfo.InvariantCulture);

            _inputValidator = new IntInputValidator(inputField, value => _intParameter.Value = value, value => _intParameter.Value = value);

            UIUtils.AddPointerListener(createKeyframeButton, EventTriggerType.PointerUp, createKeyframe);
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }
    }
}