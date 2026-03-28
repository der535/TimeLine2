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


        internal void SetValueDropdown(int value)
        {
            _dropDown.value = value;
        }
        

        

        internal void Clear()
        {
            timeInput.onEndEdit.RemoveAllListeners();
            _dropDown.onValueChanged.RemoveAllListeners();
            timeInput.text = string.Empty;
            timeInput.interactable = false;
            _dropDown.interactable = false;
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
            _dropDown.interactable = active;
        }

        internal void TimeInputOnEdit(Action<int> onValueChanged)
        {
            new IntInputValidator(timeInput, onValueChanged);
        }

    }
}