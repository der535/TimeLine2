using System;
using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.CustomInspector.UI.FieldUI
{
    public class ColorFieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform _rectTransform;

        [Space] [FormerlySerializedAs("_button")] [SerializeField]
        private Button _button;

        [FormerlySerializedAs("_text")] [SerializeField]
        private Image _colorImage;

        [Space] [SerializeField] private Button createKeyframeButton;


        private SelectColorContoller _selectColorController;
        private ColorParameter _currentParameter;

        [Inject]
        private void Constructor(SelectColorContoller selectColorController)
        {
            _selectColorController = selectColorController;
        }

        public void Setup(ColorParameter colorParameter, Action createKeyframe)
        {
            // Отписка от предыдущего параметра (если есть)
            UnsubscribeFromParameter();

            _currentParameter = colorParameter;
            _colorImage.color = _currentParameter.Value;

            _button.onClick.AddListener(OnButtonClick);
            _currentParameter.OnValueChanged += OnParameterValueChange;
            
            createKeyframeButton.onClick.AddListener(() => createKeyframe());
        }

        private void OnButtonClick()
        {
            _selectColorController?.Setup(_currentParameter);
        }

        private void OnParameterValueChange()
        {
            if (_colorImage != null)
            {
                _colorImage.color = _currentParameter.Value;
            }
        }

        private void UnsubscribeFromParameter()
        {
            if (_currentParameter != null)
            {
                _currentParameter.OnValueChanged -= OnParameterValueChange;
                _currentParameter = null;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromParameter();
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClick);
            }
        }

        public float GetFieldHeight()
        {
            return _rectTransform.sizeDelta.y;
        }
    }
}