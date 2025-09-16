using System.Globalization;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.CustomInspector.UI.FieldUI;
using TMPro;
using UnityEngine;

namespace TimeLine
{
    public class Vector2FieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform fieldRect;
        [Space]
        [SerializeField] private TextMeshProUGUI parameterName;
        [SerializeField] private TextMeshProUGUI XName;
        [SerializeField] private TextMeshProUGUI YName;
        [Space] [SerializeField] private TMP_InputField inputFieldX;
        [SerializeField] private TMP_InputField inputFieldY;

        public void Setup(Vector2Parameter parameter)
        {
            parameterName.text = parameter.Name;
            XName.text = parameter.XName;
            YName.text = parameter.YName;
            inputFieldX.text = parameter.Value.x.ToString(CultureInfo.InvariantCulture);
            inputFieldY.text = parameter.Value.y.ToString(CultureInfo.InvariantCulture);

            parameter.OnValueChanged += () =>
            {
                inputFieldX.text = parameter.Value.x.ToString(CultureInfo.InvariantCulture);
                inputFieldY.text = parameter.Value.y.ToString(CultureInfo.InvariantCulture);
            };
            
            inputFieldX.onEndEdit.AddListener(arg0 =>
                parameter.Value = new Vector2(float.Parse(arg0), parameter.Value.y));
            inputFieldY.onEndEdit.AddListener(arg0 =>
                parameter.Value = new Vector2(parameter.Value.x, float.Parse(arg0)));
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }
    }
}