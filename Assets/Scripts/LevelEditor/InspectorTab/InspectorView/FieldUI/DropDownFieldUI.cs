using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.UI.FieldUI;
using TMPro;
using UnityEngine;

namespace TimeLine
{
    public class DropDownFieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform fieldRect;
        [Space]
        [SerializeField] private TextMeshProUGUI parameterName;
        [SerializeField] private TMP_Dropdown dropdown;

        public void Setup(int startValue, List<string> options, Action<int> onValueChanged)
        {
            parameterName.text = name;
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.value = startValue;
            dropdown.onValueChanged.AddListener(onValueChanged.Invoke);
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }
    }
}