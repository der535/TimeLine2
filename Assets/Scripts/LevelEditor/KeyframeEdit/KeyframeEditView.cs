using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TimeLine.LevelEditor.KeyframeEdit
{
    public class KeyframeEditView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField timeInput;
        [SerializeField] private TextMeshProUGUI timePlaceHolder;
        [Space] 
        [SerializeField] private TMP_InputField valueInput;
        [SerializeField] private TextMeshProUGUI valuePlaceHolder;
        [Space] 
        [SerializeField] private TMP_Dropdown _dropDown;

        private void Start()
        {
            _dropDown.ClearOptions();
            _dropDown.AddOptions(new List<string>()
            {
                "---",
                Keyframe.Keyframe.InterpolationType.Linear.ToString(),
                Keyframe.Keyframe.InterpolationType.Bezier.ToString(),
                Keyframe.Keyframe.InterpolationType.Hold.ToString(),
            });
        }

        internal void InputValueOnChange(Action<float> action)
        {
            new FloatInputValidator(timeInput, action);
        }

        internal void SetValueInputInteractable(bool active)
        {
            valueInput.interactable = active;
        }

        internal void SetValueDropdown(int value)
        {
            _dropDown.value = value;
        }
        
        internal void SetTextInputValuePlaceHolder(string placeholder)
        {
            valuePlaceHolder.text = placeholder;
        }

        internal void SetTextInputValue(object data)
        {
            if(data is string text)
                valueInput.text = text;
            if(data is float floatData)
                valueInput.text = floatData.ToString();
            if (data is int intData)
                valueInput.text = intData.ToString();
        }

        internal void Clear()
        {
            timeInput.onEndEdit.RemoveAllListeners();
            valueInput.onEndEdit.RemoveAllListeners();
            _dropDown.onValueChanged.RemoveAllListeners();
            timeInput.text = string.Empty;
            valueInput.text = string.Empty;
            timeInput.enabled = false;
            valueInput.enabled = false;
            _dropDown.enabled = false;
        }
        
        internal void SelectType(Keyframe.Keyframe.InterpolationType type)
        {
            for (int i = 0; i < _dropDown.options.Count; i++)
            {
                if (_dropDown.options[i].text == type.ToString())
                {
                    _dropDown.value = i;
                    return;
                }
            }
        }

        internal void DropDownOnValueChanged(Action<Keyframe.Keyframe.InterpolationType> action)
        {
            _dropDown.onValueChanged.AddListener(index =>
            {
                if (index == 0) return; // Не меняем ничего, если выбрано "---"
                
                string selectedText = _dropDown.options[index].text;
                if (Enum.TryParse(selectedText, out Keyframe.Keyframe.InterpolationType newType))
                {
                    action.Invoke(newType);

                }
            });
        }

        internal void SetValueToTimeInput(string placeHolderText, string inputText)
        {
            
            timePlaceHolder.text = placeHolderText;
            timeInput.text = inputText;
        }

        internal void SetInteractable(bool active)
        {
            timeInput.interactable = active;
            valueInput.interactable = active;
            _dropDown.interactable = active;
        }

        internal void TimeInputOnEdit(Action<int> onValueChanged)
        {
            new IntInputValidator(timeInput, onValueChanged);
        }

    }
}