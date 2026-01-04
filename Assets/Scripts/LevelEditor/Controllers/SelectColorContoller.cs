using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;

namespace TimeLine
{
    public class SelectColorContoller : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private FlexibleColorPicker flexibleColorPicker;

        private ColorParameter _colorParameter;

        internal void Setup(ColorParameter colorParameter)
        {
            _colorParameter = colorParameter;
            rectTransform.gameObject.SetActive(true);
            flexibleColorPicker.color = colorParameter.Value;
            flexibleColorPicker.onColorChange.AddListener((color) => _colorParameter.Value = color); 
        }

        public void Close()
        {
            rectTransform.gameObject.SetActive(false);
        }

        public void Select()
        {
            _colorParameter.Value = flexibleColorPicker.color;
            Close();
        }
    }
}
