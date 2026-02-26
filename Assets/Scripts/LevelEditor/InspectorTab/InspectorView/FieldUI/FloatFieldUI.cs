using System;
using System.Globalization;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.CustomInspector.UI.FieldUI;
using TimeLine.LevelEditor.ActionHistory;
using TimeLine.LevelEditor.ActionHistory.Commands;
using TimeLine.LevelEditor.Helpers;
using TimeLine.LevelEditor.InspectorTab.Parameter;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
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
        private float _previousValue;

        [Inject]
        private void Construct(TrackObjectStorage trackObjectStorage)
        {
            _trackObjectStorage = trackObjectStorage;
        }

        public void Setup(TrackObjectPacket trackObjectPacket, BaseParameterComponent component, FloatParameter floatParameter, string gameObjectID, Action createKeyframe)
        {
            _parameter.Setup(floatParameter, trackObjectPacket, component);
            _floatParameter = floatParameter;
            parameterName.text = floatParameter.Name;
            inputField.text = floatParameter.Value.ToString(CultureInfo.InvariantCulture);
            _previousValue = _floatParameter.Value;

            _inputValidator = new FloatInputValidator(inputField,
                value =>
                {
                    CommandHistory.ExecuteCommand(new FloatParameterChangeCommand(_trackObjectStorage, floatParameter,
                        floatParameter.Name, gameObjectID, _previousValue, value));
                    _previousValue = _floatParameter.Value;
                });

            floatParameter.OnValueChanged += () =>
                inputField.text = _floatParameter.Value.ToString(CultureInfo.InvariantCulture);

            UIUtils.AddPointerListener(createKeyframeButton, EventTriggerType.PointerUp, createKeyframe);
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }
    }
}