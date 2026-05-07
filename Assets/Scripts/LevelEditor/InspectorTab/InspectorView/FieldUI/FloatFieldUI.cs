using System;
using System.Globalization;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.CustomInspector.UI.FieldUI;
using TimeLine.LevelEditor.Helpers;
using TimeLine.LevelEditor.InspectorTab.Parameter;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace TimeLine.LevelEditor.InspectorTab.InspectorView.FieldUI
{
    public class FloatFieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform fieldRect;
        [Space] [SerializeField] private TextMeshProUGUI parameterName;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private EventTrigger createKeyframeButton;
        [SerializeField] private GetParameter _parameter;

        private FloatInputValidator _inputValidator;
        private FloatParameter _floatParameter;

        private TrackObjectStorage _trackObjectStorage;
        
        public float GetFieldHeight() => fieldRect.sizeDelta.y;

        public void Setup(
            float value,
            string parameretName,
            Action createKeyframe, 
            Action<float> onValueChanged, 
            TrackObjectPacket trackObjectPacket, 
            string fieldID,
            FloatParameter onValueChangedSub = null)
        {
            _parameter.Setup(trackObjectPacket, fieldID);

            parameterName.text = parameretName;
            inputField.text = value.ToString(CultureInfo.InvariantCulture);
            
            _inputValidator = new FloatInputValidator(inputField,
                onValueChanged.Invoke);

            if (onValueChangedSub != null)
            {
                onValueChangedSub.OnValueChanged += () =>
                {
                    inputField.onEndEdit.Invoke(inputField.text);
                    _inputValidator.SetValueWithoutNotify(onValueChangedSub.Value);
                };
            }

            UIUtils.AddPointerListener(createKeyframeButton, EventTriggerType.PointerUp, createKeyframe);
        }

        private void OnDestroy()
        {
            _inputValidator.Dispose();
        }
    }
}