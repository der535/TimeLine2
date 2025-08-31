using System;
using System.Collections.Generic;
using TimeLine.Installers;
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
                "None",
                "1/64",
                "1/48",
                "1/32",
                "1/24",
                "1/16",
                "1/12",
                "1/8",
                "1/6",
                "1/4",
                "1/3",
                "1/2",
                "Step",
                "Whole Note"
            });
            
            dropdown.onValueChanged.AddListener(arg0 =>
            {
                switch (dropdown.options[arg0].text)
                {
                    case "None":
                        gridUI.GridSize = 1f/(float)Main.TICKS_PER_BEAT;
                        break;
                    case "1/64":
                        gridUI.GridSize = 1f/64f;
                        break;
                    case "1/48":
                        gridUI.GridSize = 1f/48f;
                        break;
                    case "1/32":
                        gridUI.GridSize = 1f/32f;
                        break;
                    case "1/24":
                        gridUI.GridSize = 1f/24f;
                        break;
                    case "1/16":
                        gridUI.GridSize = 1f/16f;
                        break;
                    case "1/12":
                        gridUI.GridSize = 1f/12f;
                        break;
                    case "1/8":
                        gridUI.GridSize = 1f/8f;
                        break;
                    case "1/6":
                        gridUI.GridSize = 1f/6f;
                        break;
                    case "1/4":
                        gridUI.GridSize = 1f/4f;
                        break;
                    case "1/3":
                        gridUI.GridSize = 1f/3f;
                        break;
                    case "1/2":
                        gridUI.GridSize = 1f/2f;
                        break;
                    case "Step":
                        gridUI.GridSize = 1f;
                        break;
                    case "Whole Note":
                        gridUI.GridSize = 4f; // 4 beats for a whole note in 4/4 time
                        break;
                }
            });
            
            // Set default value to 1/4 note
            dropdown.value = dropdown.options.FindIndex(option => option.text == "1/4");
        }
    }
}