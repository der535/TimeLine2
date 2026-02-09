using System;
using EventBus;
using TimeLine.CustomInspector.UI.FieldUI;
using TimeLine.EventBus.Events.Misc;
using TimeLine.LevelEditor.General;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.InspectorView.FieldUI
{
    public class EditColliderFieldUI : MonoBehaviour, IGetFieldHeight
    {
        [SerializeField] private RectTransform fieldRect;
        [Space]
        [SerializeField] private TextMeshProUGUI parameterName;
        [SerializeField] private Button button;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Color editColor;
        [SerializeField] private Color notEditColor;
        
        private C_EditColliderState _cEditState;
        private GameEventBus _eventBus;
        
        private EventBinder _eventBinder = new EventBinder();

        [Inject]
        private void Constructor(C_EditColliderState cEditColliderState, GameEventBus eventBus)
        {
            _cEditState = cEditColliderState;
            _eventBus = eventBus;
        }
        
        public void Setup()
        {
            button.onClick.AddListener(() =>
            {
                _cEditState.Turn(!_cEditState.GetState());
            });

            _eventBinder.Add(_eventBus, (ref TurnEditColliderEvent data) =>
            {
                buttonImage.color = data.IsEditing ? editColor : notEditColor;
            });
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }

        private void OnDestroy()
        {
            _eventBinder.Dispose();
        }
    }
}