using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class ThemeController : MonoBehaviour
    {
        [SerializeField] private ThemeView themeView;
        [SerializeField] private ThemeStorage themeStorage;

        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref ThemeChangedEvent data) =>
            {
                themeStorage.value = data.Theme;
                Paint();
            });

            _gameEventBus.Raise(new ThemeChangedEvent(themeStorage.value));
        }

        private void Paint()
        {
            Paint(themeView.primaryColor, themeStorage.value.primary);
            Paint(themeView.secondaryColor, themeStorage.value.secondary);
            Paint(themeView.icons, themeStorage.value.icons);
            Paint(themeView.inputFields, themeStorage.value.inputFieldBackground, themeStorage.value.inputFieldText,
                themeStorage.value.inputFieldTextPlaceholder);
            Paint(themeView.currentTimeMarker, themeStorage.value.currentTimeLine);
            Paint(themeView.buttons, themeStorage.value.bacroundButtons, themeStorage.value.textButtons);
            Paint(themeView.texts, themeStorage.value.textColor);
            Paint(themeView.tabs, themeStorage.value.tabs);
            Paint(themeView.sliderPaints, themeStorage.value.sliderBacround, themeStorage.value.sliderHandle);
            Paint(themeView.dropDowns,
                themeStorage.value.dropDownBackground,
                themeStorage.value.dropDownText,
                themeStorage.value.dropDownArrow,
                themeStorage.value.dropDownItemBackground,
                themeStorage.value.dropDownItemCheckmark,
                themeStorage.value.dropDownItemLabel);
            themeView.iconPlay.color = themeStorage.value.iconPlay;
            themeView.sceneBackground.color = themeStorage.value.backgroundSceneColor;
            themeView.grid.gridColor = themeStorage.value.gridSceneColor;
        }

        private void Paint(List<Image> images, Color bg)
        {
            foreach (var item in images)
            {
                item.color = bg;
            }
        }

        private void Paint(List<TextMeshProUGUI> images, Color bg)
        {
            foreach (var item in images)
            {
                item.color = bg;
            }
        }

        private void Paint(
            List<M_DropDownPaint> images,
            Color dropDownBackground,
            Color dropDownText,
            Color dropDownArrow,
            Color dropDownItemBackground,
            Color dropDownItemCheckmark,
            Color dropDownItemLabel)
        {
            foreach (var item in images)
            {
                item.background.color = dropDownBackground;
                item.text.color = dropDownText;
                item.arrow.color = dropDownArrow;
                item.itemBackground.color = dropDownItemBackground;
                item.itemCheckmark.color = dropDownItemCheckmark;
                item.itemLabel.color = dropDownItemLabel;
            }
        }

        private void Paint(List<M_InputFieldPaint> inputField, Color bg, Color mainText, Color placeHolder)
        {
            foreach (var item in inputField)
            {
                item.background.color = bg;
                item.mainText.color = mainText;
                if (item.placeHolder) item.placeHolder.color = placeHolder;
            }
        }
        
        private void Paint(List<M_SliderPaint> inputField, Color bg, Color handle)
        {
            foreach (var item in inputField)
            {
                item.background.color = bg;
                item.handle.color = handle;
            }
        }

        private void Paint(List<M_ButtonsPaint> inputField, Color bg, Color text)
        {
            foreach (var item in inputField)
            {
                item.background.color = bg;
                item.text.color = text;
            }
        }
    }
}