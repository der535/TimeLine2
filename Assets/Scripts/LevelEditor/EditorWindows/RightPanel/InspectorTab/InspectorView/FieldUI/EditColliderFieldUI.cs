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
        
        private EditColliderState _editState;
        private GameEventBus _eventBus;

        [Inject]
        private void Constructor(EditColliderState editColliderState, GameEventBus eventBus)
        {
            _editState = editColliderState;
            _eventBus = eventBus;
        }
        
        public void Setup()
        {
            button.onClick.AddListener(() =>
            {
                _editState.Turn(!_editState.GetState());
            });
            
            _eventBus.SubscribeTo((ref TurnEditColliderEvent data) =>
            {
                buttonImage.color = data.IsEditing ? editColor : notEditColor;
            });
        }

        public float GetFieldHeight()
        {
            return fieldRect.sizeDelta.y;
        }
    }
}