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
    public class KeyCodeFieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform fieldRect;
        [Space]
        [SerializeField] private TextMeshProUGUI parameterName;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private Button readKey;
        [SerializeField] private Image readKeyImage;
        [SerializeField] private Color readKeyColor;
        [SerializeField] private Color notReadKeyColor;
        [SerializeField] private EventTrigger createKeyframeButton;
        
        private KeyCodeParameter _keyCodeParameter;
        private bool _isReadingKey;

        public void Setup(KeyCodeParameter floatParameter, Action createKeyframe)
        {
            _keyCodeParameter = floatParameter;

            readKey.onClick.AddListener(ListenButton);
            
            UIUtils.AddPointerListener(createKeyframeButton, EventTriggerType.PointerUp, createKeyframe);
        }

        private void ListenButton()
        {
            if (!_isReadingKey)
            {
                _isReadingKey = true;
                buttonText.text = "Reading...";
            }
        }

        public void Update()
        {
            if(!_isReadingKey) return;
            
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (UnityEngine.Input.GetKeyDown(key))
                {
                    Debug.Log("Нажата клавиша: " + key);
                    _keyCodeParameter.Value = key;
                    buttonText.text = key.ToString();
                    _isReadingKey = false;
                }
            }
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }
    }
}