using System;
using System.Globalization;
using TimeLine.CustomInspector.Logic.Parameter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class BoolFieldUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI parameterName;
        [SerializeField] private Toggle toggle;

        public void Setup(BoolParameter boolParameter)
        {
            parameterName.text = boolParameter.Name;
            toggle.isOn = boolParameter.Value;
            
            boolParameter.OnValueChanged += () => toggle.isOn = boolParameter.Value;

            toggle.onValueChanged.AddListener((arg0 => boolParameter.Value = arg0));
        }
    }
}