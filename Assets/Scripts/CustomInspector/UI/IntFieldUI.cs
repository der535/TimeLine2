using System;
using TMPro;
using UnityEngine;

namespace TimeLine
{
    public class IntFieldUI : FieldUIBase<int>
    {
        [SerializeField] private TMP_InputField inputField;
        public override void Setup(IField<int> field, Action onValueChanged)
        {
            base.Setup(field, onValueChanged);
            
            inputField.text = field.Value.ToString();
            inputField.onEndEdit.AddListener(arg0 =>
            {
                field.Value = int.Parse(arg0);
                onValueChanged.Invoke();
            });
        }
    }
}