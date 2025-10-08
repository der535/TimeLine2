using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class LevelCard : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _text;

        internal void Setup(string text, Action onClick)
        {
            _button.onClick.AddListener(onClick.Invoke);
            _text.text = text;
        }
    }
}
