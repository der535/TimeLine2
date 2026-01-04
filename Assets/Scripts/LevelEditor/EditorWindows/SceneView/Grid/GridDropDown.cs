using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.Installers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class GridDropDown : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;

        [FormerlySerializedAs("gridSystem")] [SerializeField]
        private GridUI gridUI;

        internal int GetGridSize() => dropdown.value;
        internal void SetGridSize(int gridSize)
        {
            dropdown.value = gridSize;
        }

        private SaveEditorSettings _saveEditorSettings;

        [Inject]
        private void Constructor(SaveEditorSettings saveEditorSettings)
        {
            _saveEditorSettings = saveEditorSettings;
        }

        private void Awake()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>()
            {
                "None",
                "Step",
                "Whole Note",
                "1/2",
                "1/3",
                "1/4",
                "1/6",
                "1/8",
                "1/12",
                "1/16",
                "1/24",
                "1/32",
                "1/48",
                "1/64"
            });

            dropdown.onValueChanged.AddListener(arg0 =>
            {
                switch (dropdown.options[arg0].text)
                {
                    case "None":
                        gridUI.GridSize = 1f / (float)Main.TICKS_PER_BEAT;
                        break;
                    case "1/64":
                        gridUI.GridSize = 1f / 64f;
                        break;
                    case "1/48":
                        gridUI.GridSize = 1f / 48f;
                        break;
                    case "1/32":
                        gridUI.GridSize = 1f / 32f;
                        break;
                    case "1/24":
                        gridUI.GridSize = 1f / 24f;
                        break;
                    case "1/16":
                        gridUI.GridSize = 1f / 16f;
                        break;
                    case "1/12":
                        gridUI.GridSize = 1f / 12f;
                        break;
                    case "1/8":
                        gridUI.GridSize = 1f / 8f;
                        break;
                    case "1/6":
                        gridUI.GridSize = 1f / 6f;
                        break;
                    case "1/4":
                        gridUI.GridSize = 1f / 4f;
                        break;
                    case "1/3":
                        gridUI.GridSize = 1f / 3f;
                        break;
                    case "1/2":
                        gridUI.GridSize = 1f / 2f;
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

            dropdown.onValueChanged.AddListener((_) =>
            {
                _saveEditorSettings.Save();
            });
        }
    }
}