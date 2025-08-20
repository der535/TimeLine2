using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TimeLine
{
    public class CoordinateSystem : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;

        public bool IsGlobal { get; private set; }
        public Action<bool> OnCoordinateChanged { get; set; }


        private void Start()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string> { "Global", "Local" });
            dropdown.onValueChanged.AddListener(arg0 =>
            {
                IsGlobal = arg0 == 0;
                OnCoordinateChanged?.Invoke(IsGlobal);
            });
            IsGlobal = true;
        }
    }
}
