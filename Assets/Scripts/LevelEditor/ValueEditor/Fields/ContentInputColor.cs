using System;
using TimeLine.CustomInspector.Logic.Parameter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor.Fields
{
    public class ContentInputColor : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private Image colorPreview;

        private SelectColorContoller _selectColorController;
        private ColorParameter _colorParameter = new("Color", Color.white, Color.white);

        [Inject]
        private void Construct(SelectColorContoller selectColorController)
        {
            _selectColorController = selectColorController;
        }

        internal void Setup(Color value, string lable, Action<Color> onValueChanged)
        {
            _colorParameter.Value = value;
            colorPreview.color = value;
            _colorParameter.OnValueChanged += () =>
            {
                colorPreview.color = _colorParameter.Value;
                onValueChanged.Invoke(_colorParameter.Value);
            };

            buttonText.text = lable;
            button.onClick.RemoveAllListeners();
            // button.onClick.AddListener(() => { _selectColorController.Setup(_colorParameter, string.Empty); });
        }
    }
}