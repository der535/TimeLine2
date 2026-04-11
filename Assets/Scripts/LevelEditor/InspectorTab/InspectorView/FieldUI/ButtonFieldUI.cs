using System;
using EventBus;
using TimeLine.CustomInspector.UI.FieldUI;
using TimeLine.EventBus.Events.Misc;
using TimeLine.LevelEditor.InspectorTab.Components.EdgeCollider;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.InspectorView.FieldUI
{
    public class ButtonFieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform fieldRect;
        [Space]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI ButtonText;
        
        public void Setup(Action action, string buttonText)
        {
            button.onClick.AddListener(action.Invoke);
            ButtonText.text = buttonText;
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }
    }
}