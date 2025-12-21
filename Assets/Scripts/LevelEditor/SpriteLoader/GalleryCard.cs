using System;
using EventBus;
using TimeLine.CustomInspector.Logic.Parameter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.LevelEditor.SpriteLoader
{
    public class GalleryCard : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private SetNativeSize setNativeSize;
        [SerializeField] private new TextMeshProUGUI name;
        [SerializeField] private Button button;
        [SerializeField] private Button deleteButton;
        
        private GameEventBus _gameEventBus;
        private SpriteParameter _spriteParameter;
        private TextureData _textureData;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        public void Setup(SpriteParameter parameter, TextureData textureData, Action edit, Action delete)
        {
            _spriteParameter = parameter;
            _textureData = textureData;
            
            image.sprite = parameter.Value;
            name.text = textureData.SpriteName;
            setNativeSize.Init();
            button.onClick.AddListener(edit.Invoke);
            deleteButton.onClick.AddListener(delete.Invoke);
            parameter.OnValueChanged += () =>
            {
                image.sprite = parameter.Value;
            };
        }

        public void UpdateCard()
        {
            image.sprite = _spriteParameter.Value;
            name.text = _textureData.SpriteName;
            setNativeSize.Init();
        }
    }
}