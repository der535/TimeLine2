using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.CustomInspector.UI.FieldUI
{
    public class ColorFieldUI: MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform _rectTransform;
        [Space]
        [FormerlySerializedAs("_button")] [SerializeField] private Button button;
        [FormerlySerializedAs("_text")] [SerializeField] private Image colorImage;

        private SelectColorContoller _selectColorController;

        [Inject]
        private void Constructor(SelectColorContoller selectColorController)
        {
            _selectColorController = selectColorController;
        }
        
        public void Setup(ColorParameter colorParameter)
        {
            colorImage.color = colorParameter.Value;

            button.onClick.AddListener(() =>
            {
                _selectColorController.Setup(colorParameter);
            });
            
            colorParameter.OnValueChanged += () => { colorImage.color = colorParameter.Value; };
        }

        public float GetFieldHeight()
        {
            return _rectTransform.sizeDelta.y;
        }
    }
}