using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace TimeLine.LevelEditor.ValueEditor
{
    public class ContentButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI buttonText;
        
        internal void Setup(string text, Action onPress)
        {
            // Устанавливаем начальное значение
            buttonText.text = text;
            button.onClick.AddListener(onPress.Invoke);
        }
    }
}