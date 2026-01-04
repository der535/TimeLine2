using System;
using System.Collections.Generic;
using TimeLine.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class ComponentLine : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Button button;

        internal void Setup(string componentName, Action onClick)
        {
            text.text = componentName;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick.Invoke);
        }
    }
}
