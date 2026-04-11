using System;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.SpriteLoader;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor.Fields
{
    public class ContentInputSprite : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI buttonText;

        private GetSpriteName _getSpriteName;
        private SelectSpriteController _selectSpriteController;

        [Inject]
        private void Construct(SelectSpriteController selectSpriteController, GetSpriteName getSpriteName)
        {
            _selectSpriteController = selectSpriteController;
            _getSpriteName = getSpriteName;
        }

        internal void Setup(string spriteID, string lable, Action<string> onValueChanged)
        {
           
           

            buttonText.text = lable;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                _selectSpriteController.Setup(_getSpriteName.GetSpriteFromName(spriteID), texture =>
                {
                    onValueChanged.Invoke(texture.name);
                    buttonText.text = texture.name;
                });
            });
        }
    }
}