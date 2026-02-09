using System;
using TMPro;
using UnityEngine;

namespace TimeLine.LevelEditor.ValueEditor
{
    public class ContentInputFloat : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        private FloatInputValidator _floatInputValidator;
        
        internal void Setup(float value, Action<float> onValueChanged)
        {
            // Устанавливаем начальное значение
            inputField.text = value.ToString();

            _floatInputValidator = new FloatInputValidator(inputField, onValueChanged);
        }
    }
}