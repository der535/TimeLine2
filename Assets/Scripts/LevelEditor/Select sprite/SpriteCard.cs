using System;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.SpriteLoader;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class SpriteCard : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMPro.TextMeshProUGUI text;
        [SerializeField] private Button button;
        
        public Sprite sprite;
        
        private Action onValueChanged;
        public SpriteParameter SpriteParameter;
        public TextureData textureData;

        internal void Setup(Sprite sprite, Action onClick)
        {
            this.sprite = sprite;
            image.sprite = sprite;
            text.text = sprite.name;
            if (onClick != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(onClick.Invoke);
            }
        }
        
        internal void Setup(SpriteParameter spriteParameter, TextureData textureData, Action onClick)
        {
            this.SpriteParameter = spriteParameter;
            this.textureData = textureData;
            sprite = spriteParameter.Value;
            image.sprite = spriteParameter.Value;
            text.text = textureData.SpriteName;
            
            SpriteParameter.OnValueChanged -= onValueChanged;
            
            onValueChanged = () =>
            {
                sprite = spriteParameter.Value;
                image.sprite = spriteParameter.Value;
                text.text = textureData.SpriteName;
            };
           
            spriteParameter.OnValueChanged += onValueChanged;

            if (onClick != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(onClick.Invoke);
            }
        }

        private void OnDestroy()
        {
            if(SpriteParameter != null) SpriteParameter.OnValueChanged -= onValueChanged;
        }
    }
}
