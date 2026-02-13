using System;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.CustomInspector.UI.FieldUI
{
    public class ColorFieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform _rectTransform; //rectTransform для считываения размера панели 
        [Space] [SerializeField] private Button _button; // Кнопка открытия цветовой палитры
        [FormerlySerializedAs("_text")] [SerializeField]
        private Image _colorImage; //Image в котором отобращается выбранный цвет

        [Space] [SerializeField] private EventTrigger createKeyframeButton;
        
        private SelectColorContoller _selectColorController;
        private ColorParameter _currentParameter;
        
        private Color _previousValue;
        
        [Inject]
        private void Constructor(SelectColorContoller selectColorController)
        {
            _selectColorController = selectColorController;
        }

        public void Setup(ColorParameter colorParameter, Action createKeyframe, string gameObjectId)
        {
            // Отписка от предыдущего параметра (если есть)
            UnsubscribeFromParameter();

            _currentParameter = colorParameter; //Сохраняем ссылку на параметр
            _colorImage.color = _currentParameter.Value; //Задаём цвет image

            _button.onClick.AddListener(() => OnButtonClick(gameObjectId));
            _currentParameter.OnValueChanged += OnParameterValueChange;
            
            UIUtils.AddPointerListener(createKeyframeButton, EventTriggerType.PointerUp, () => createKeyframe?.Invoke());
        }
        
        private void OnButtonClick(string gameObjectId)
        {
            _selectColorController?.Setup(_currentParameter, gameObjectId);
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
                _button.onClick.RemoveAllListeners();
            }
        }

        public float GetFieldHeight()
        {
            return _rectTransform.sizeDelta.y;
        }
    }
}