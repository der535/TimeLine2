using System;
using System.Globalization;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.CustomInspector.UI.FieldUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class StringFieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform fieldRect;
        [Space]
        [SerializeField] private TextMeshProUGUI parameterName;
        [SerializeField] private TMP_InputField inputField;

        public void Setup(StringParameter stringParameter)
        {
            parameterName.text = stringParameter.Name;
            inputField.text = stringParameter.Value;
            
            stringParameter.OnValueChanged += () => inputField.text = stringParameter.Value.ToString(CultureInfo.InvariantCulture);

            inputField.onEndEdit.AddListener(arg0 => stringParameter.Value = arg0);
        }
        
        public void Setup(string value, string paremeterName, Action<string> onValueChanged)
        {
            parameterName.text = paremeterName;
            inputField.text = value;
            
            inputField.onEndEdit.AddListener(onValueChanged.Invoke);
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }
    }
}