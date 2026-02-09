using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.CustomInspector.UI.FieldUI;
using TimeLine.LevelEditor.Select_composition;
using TimeLine.LevelEditor.SpriteLoader;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class CompositionFieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform _rectTransform;
        [Space]
        [FormerlySerializedAs("_button")] [SerializeField] private Button button;
        [FormerlySerializedAs("_text")] [SerializeField] private TextMeshProUGUI text;

        private SelectComposition _selectCompositionController;
        private GetSpriteName _getSpriteName;

        [Inject]
        private void Constructor(SelectComposition selectCompositionConstroller, CustomSpriteStorage customSpriteStorage, GetSpriteName getSpriteName)
        {
            _selectCompositionController = selectCompositionConstroller;
            _getSpriteName = getSpriteName;
        }
        
        public void Setup(CompositionParameter spriteParameter)
        {
            if(spriteParameter.Value != null)
                text.text = spriteParameter.Value.gameObjectName;

            button.onClick.AddListener(() =>
            {
                _selectCompositionController.Setup(spriteParameter);
            });
            
            spriteParameter.OnValueChanged += () => { text.text = spriteParameter.Value.gameObjectName; };
        }

        public float GetFieldHeight()
        {
            return _rectTransform.sizeDelta.y;
        }
    }
}