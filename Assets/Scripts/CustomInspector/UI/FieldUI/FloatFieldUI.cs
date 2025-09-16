using System;
using System.Globalization;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.CustomInspector.UI.FieldUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class FloatFieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform fieldRect;
        [Space]
        [SerializeField] private TextMeshProUGUI parameterName;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button createKeyframeButton;

        public void Setup(FloatParameter floatParameter, Action createKeyframe)
        {
            parameterName.text = floatParameter.Name;
            inputField.text = floatParameter.Value.ToString(CultureInfo.InvariantCulture);
            
            floatParameter.OnValueChanged += () => inputField.text = floatParameter.Value.ToString(CultureInfo.InvariantCulture);

            inputField.onEndEdit.AddListener(arg0 => floatParameter.Value = float.Parse(arg0));
            createKeyframeButton.onClick.AddListener(() => createKeyframe());
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }
    }
}