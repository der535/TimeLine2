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
        private Action<Color> OnCnageColor;
        
        private Color _previousValue;
        
        [Inject]
        private void Constructor(SelectColorContoller selectColorController)
        {
            _selectColorController = selectColorController;
        }
        
        public void Setup(Action<Color> onCnageColor, Color startColor, Action createKeyframe)
        {
            // Отписка от предыдущего параметра (если есть)
            OnCnageColor = null;
            OnCnageColor += (value) => _colorImage.color = value;
            OnCnageColor += onCnageColor;
            
            _colorImage.color = startColor; //Задаём цвет image

            _button.onClick.AddListener(() => OnButtonClick(startColor));

            
            UIUtils.AddPointerListener(createKeyframeButton, EventTriggerType.PointerUp, () => createKeyframe?.Invoke());
        }

        
        private void OnButtonClick(Color startColor)
        {
            _selectColorController?.Setup(OnCnageColor, startColor);
        }
        
        private void OnDestroy()
        {
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