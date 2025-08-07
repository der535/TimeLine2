using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace TimeLine
{
    public class FloatFieldUI : FieldUIBase<float>
    {
        [SerializeField] private TMP_InputField inputField;
        public override void Setup(IField<float> field, Action onChangeCustomInspector)
        {
            base.Setup(field, onChangeCustomInspector);
            
            inputField.text = field.Value.ToString(CultureInfo.InvariantCulture);
            inputField.onEndEdit.AddListener(arg0 =>
            {
                field.Value = float.Parse(arg0);
                onChangeCustomInspector.Invoke();
            });
        }
    }
}