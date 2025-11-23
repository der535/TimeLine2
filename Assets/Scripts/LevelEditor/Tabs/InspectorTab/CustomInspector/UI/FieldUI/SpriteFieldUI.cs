using System.Globalization;
using TimeLine.CustomInspector.Logic.Parameter;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.CustomInspector.UI.FieldUI
{
    public class SpriteFieldUI: MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform _rectTransform;
        [Space]
        [FormerlySerializedAs("_button")] [SerializeField] private Button button;
        [FormerlySerializedAs("_text")] [SerializeField] private TextMeshProUGUI text;

        private SelectSpriteController _selectSpriteController;

        [Inject]
        private void Constructor(SelectSpriteController selectSpriteController)
        {
            _selectSpriteController = selectSpriteController;
        }
        
        public void Setup(SpriteParameter spriteParameter)
        {
            // print("CreateSpriteField");

            text.text = spriteParameter.Name;

            button.onClick.AddListener(() =>
            {
                _selectSpriteController.Setup(spriteParameter);
            });
            
            spriteParameter.OnValueChanged += () => { text.text = spriteParameter.Value.name; };
        }

        public float GetFieldHeight()
        {
            return _rectTransform.sizeDelta.y;
        }
    }
}