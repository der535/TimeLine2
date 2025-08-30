using System;
using System.Globalization;
using TimeLine.CustomInspector.Logic.Parameter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class StringFieldUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI parameterName;
        [SerializeField] private TMP_InputField inputField;

        public void Setup(StringParameter stringParameter)
        {
            parameterName.text = stringParameter.Name;
            inputField.text = stringParameter.Value;
            
            stringParameter.OnValueChanged += () => inputField.text = stringParameter.Value.ToString(CultureInfo.InvariantCulture);

            inputField.onEndEdit.AddListener(arg0 => stringParameter.Value = arg0);
        }
    }
}