using System;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine.LevelEditor.AuxiliaryWindowsController
{
    public class AuxiliaryWindowsController : MonoBehaviour
    {
        [SerializeField] private RectTransform auxiliaryWindowsParent;
        [SerializeField] private Button changeActive;

        private void Start()
        {
            changeActive.onClick.AddListener(ChangeActive);
        }

        private void ChangeActive()
        {
            auxiliaryWindowsParent.gameObject.SetActive( !auxiliaryWindowsParent.gameObject.activeSelf);
        }
    }
}