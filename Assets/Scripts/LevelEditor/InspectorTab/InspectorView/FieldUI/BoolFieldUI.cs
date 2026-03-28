using System;
using NaughtyAttributes;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.CustomInspector.UI.FieldUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class BoolFieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform fieldRect;
        [Space] [SerializeField] private TextMeshProUGUI parameterName;
        [SerializeField] private Toggle toggle;

        public void Setup(BoolParameter boolParameter)
        {
            parameterName.text = boolParameter.Name;
            toggle.isOn = boolParameter.Value;

            boolParameter.OnValueChanged += () => toggle.isOn = boolParameter.Value;

            toggle.onValueChanged.AddListener((arg0 => boolParameter.Value = arg0));
        }

        public void Setup(bool startValue, string parameterNam, Action<bool> onValueChanged)
        {
            parameterName.text = parameterNam;
            toggle.isOn = startValue;

            toggle.onValueChanged.AddListener((onValueChanged.Invoke));
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }
    }
}