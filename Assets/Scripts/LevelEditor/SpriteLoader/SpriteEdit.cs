using System;
using System.Collections.Generic;
using System.Globalization;
using TimeLine.CustomInspector.Logic.Parameter;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TimeLine.LevelEditor.SpriteLoader
{
    public class SpriteEdit : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Dropdown filterMode;
        [SerializeField] private TMP_InputField pixelsPerUnit;
        [SerializeField] private TMP_InputField spriteName;
        [Space] 
        [SerializeField] private GameObject selectCard;
        [SerializeField] private GameObject editCard;
        [FormerlySerializedAs("spriteStorage")] [SerializeField] private CustomSpriteStorage customSpriteStorage;
        
        private FloatInputValidator _inputValidator;
        private StringInputValidator _stringInputValidator;
        
        private Action _onValueChanged;
        private SpriteParameter _savedSpriteParameter;
        
        private void Start()
        {
            SetupFilterMode();
        }

        private void SetupFilterMode()
        {
            filterMode.ClearOptions();
            var options = new List<TMP_Dropdown.OptionData>
            {
                new(FilterMode.Bilinear.ToString()),
                new(FilterMode.Trilinear.ToString()),
                new(FilterMode.Point.ToString())
            };
            filterMode.AddOptions(options);
        }
        
        private void SetFilterMode(string mode)
        {
            for (int i = 0; i < filterMode.options.Count; i++)
            {
                if (filterMode.options[i].text == mode)
                {
                    print(i);
                    filterMode.value = i;
                    return;
                }
            }
        }
        
        public void Setup(SpriteParameter spriteParameter, TextureData textureData, Action updateCard)
        {
            _inputValidator?.Dispose();
            _stringInputValidator?.Dispose();
            filterMode.onValueChanged.RemoveAllListeners();
            
            _savedSpriteParameter = spriteParameter;
            _onValueChanged = () => image.sprite = spriteParameter.Value; 
            
            spriteParameter.OnValueChanged += _onValueChanged;

            image.sprite = spriteParameter.Value;
            SetFilterMode(textureData.FilterMode.ToString());
            pixelsPerUnit.text = textureData.PixelsPerUnit.ToString(CultureInfo.InvariantCulture);
            spriteName.text = textureData.SpriteName;
            
            _inputValidator = new FloatInputValidator(pixelsPerUnit, value =>
            {
                textureData.PixelsPerUnit = value;
                customSpriteStorage.UpdateCard(textureData);
                updateCard.Invoke();
            });
            _stringInputValidator = new StringInputValidator(spriteName,  value =>
            {
                textureData.SpriteName = value;
                customSpriteStorage.UpdateCard(textureData);
                updateCard.Invoke();
            });
            filterMode.onValueChanged.AddListener((value) =>
            {
                SetFilterMode(filterMode.options[value].text);
                
                if (Enum.TryParse<FilterMode>(filterMode.options[value].text, out FilterMode mode))
                {
                    textureData.FilterMode = mode;
                }
                else
                {
                    Debug.LogWarning("Invalid filter mode: " + filterMode.options[value].text);
                }
                
                customSpriteStorage.UpdateCard(textureData);
                updateCard.Invoke();
            });
            
            selectCard.SetActive(false);
            editCard.SetActive(true);
        }

        public void Dispose()
        {
            _inputValidator?.Dispose();
            _stringInputValidator?.Dispose();
            filterMode.onValueChanged.RemoveAllListeners();
            _savedSpriteParameter.OnValueChanged -= _onValueChanged;
        }
    }
}