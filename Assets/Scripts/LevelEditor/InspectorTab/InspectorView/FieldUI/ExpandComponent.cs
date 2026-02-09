using System;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class ExpandComponent : MonoBehaviour
    {
        [SerializeField] private GameObject triangle;
        [SerializeField] private Button button;
        [SerializeField] private ComponentUI componentUI;

        private bool _isExpanded = true;

        private void Start()
        {
            button.onClick.AddListener(ChangeVisibility);
        }

        public void ChangeVisibility() //Используется кно
        {
            _isExpanded = !_isExpanded;

            if (!_isExpanded)
            {
                triangle.transform.localRotation = Quaternion.Euler(0, 0, 270);
                componentUI.Hide();
            }
            else
            {
                triangle.transform.localRotation = Quaternion.Euler(0, 0, 180);
                componentUI.Show();
            }
        }
    }
}
