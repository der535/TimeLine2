using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace TimeLine
{
    public class GridDropDown : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [FormerlySerializedAs("gridSystem")] [SerializeField] private GridUI gridUI;

        private void Start()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>()
            {
                "none",
                "1/6 step",
                "1/4 step",
                "1/3 step",
                "1/2 step",
                "Step",
                "1/6 beat",
                "1/4 beat",
                "1/3 beat",
                "1/2 beat",
                "Beat",
                "Bar",
            });
            dropdown.onValueChanged.AddListener(arg0 =>
            {
                switch (dropdown.options[arg0].text)
                {
                    case "none":
                        gridUI.GridSize = 1;
                        break;
                    case "1/6 step":
                        gridUI.GridSize = 1000/4/4/6;
                        break;
                    case "1/4 step":
                        gridUI.GridSize = 1000/4/4/4;
                        break;
                    case "1/3 step":
                        gridUI.GridSize = 1000/4/4/3;
                        break;
                    case "1/2 step":
                        gridUI.GridSize = 1000/4/4/2;
                        break;
                    case "Step":
                        gridUI.GridSize = 1000/4/4;
                        break;
                    case "1/6 beat":
                        gridUI.GridSize = 1000/4/6;
                        break;
                    case "1/4 beat":
                        gridUI.GridSize = 1000/4/4;
                        break;
                    case "1/3 beat":
                        gridUI.GridSize = 1000/4/3;
                        break;
                    case "1/2 beat":
                        gridUI.GridSize = 1000/4/2;
                        break;
                    case "Beat":
                        gridUI.GridSize = 1000/4;
                        break;
                    case "Bar":
                        gridUI.GridSize = 1000;
                        break;
                }
            });
            dropdown.value = 10;
        }
    }
}