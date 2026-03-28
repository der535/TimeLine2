using System;
using System.Globalization;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.SpriteLoader;
using TMPro;
using Unity.Rendering;
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
        private CustomSpriteStorage _customSpriteStorage;
        private GetSpriteName _getSpriteName;

        [Inject]
        private void Constructor(SelectSpriteController selectSpriteController, CustomSpriteStorage customSpriteStorage, GetSpriteName getSpriteName)
        {
            _selectSpriteController = selectSpriteController;
            _customSpriteStorage = customSpriteStorage;
            _getSpriteName = getSpriteName;
        }
        
        public void Setup(SpriteParameter spriteParameter)
        {
            text.text = _getSpriteName.Get(spriteParameter);

            button.onClick.AddListener(() =>
            {
                _selectSpriteController.Setup(spriteParameter);
            });
            
            spriteParameter.OnValueChanged += () => { text.text = _getSpriteName.Get(spriteParameter); };
        }
        
        public void Setup(Sprite spriteParameter, Action<Texture> onValueChanged)
        {
            string name = "Null";
            if (spriteParameter != null) name = _getSpriteName.Get(spriteParameter.name);
            text.text = name;
        
            button.onClick.AddListener(() =>
            {
                _selectSpriteController.Setup(spriteParameter, onValueChanged);
            });
        }

        public float GetFieldHeight()
        {
            return _rectTransform.sizeDelta.y;
        }
    }
}