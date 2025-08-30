using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TimeLine.CustomInspector.Logic.Parameter;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TimeLine
{
    public class DropDownFieldUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI parameterName;
        [FormerlySerializedAs("inputField")] [SerializeField] private TMP_Dropdown dropdown;

        public TMP_Dropdown Setup(string name)
        {
            parameterName.text = name;
            print(dropdown);
            return dropdown;
        }
    }
}