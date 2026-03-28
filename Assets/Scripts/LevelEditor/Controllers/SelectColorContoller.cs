using System;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.ActionHistory;
// using TimeLine.LevelEditor.ActionHistory.Commands;
using UnityEngine;

namespace TimeLine
{
    public class SelectColorContoller : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private FlexibleColorPicker flexibleColorPicker;

        private Action<Color> OnColorChanged;

        private bool _ignorFirstChange = true;

        private void Start()
        {
            flexibleColorPicker.onColorChange.AddListener((value) =>
            {
                if (_ignorFirstChange)
                {
                    _ignorFirstChange = false;
                }
                else
                {
                    OnColorChanged.Invoke(value);
                }
            });
        }


        internal void Setup(Action<Color> colorParameter, Color startColor)
        {
            OnColorChanged = colorParameter;
            rectTransform.gameObject.SetActive(true);
            flexibleColorPicker.color = startColor;
        }

        public void Close()
        {
            _ignorFirstChange = true;
            rectTransform.gameObject.SetActive(false);
        }

        public void Select()
        {
            Close();
        }
    }
}