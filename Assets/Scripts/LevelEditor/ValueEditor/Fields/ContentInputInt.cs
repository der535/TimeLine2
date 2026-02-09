using System;
using TMPro;
using UnityEngine;

namespace TimeLine.LevelEditor.ValueEditor
{
    public class ContentInputInt : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        
        internal void Setup(int value, Action<int> onValueChanged)
        {
            // Устанавливаем начальное значение
            inputField.text = value.ToString();

            IntInputValidator inputValidator = new IntInputValidator(inputField, onValueChanged);
        }
    }
}