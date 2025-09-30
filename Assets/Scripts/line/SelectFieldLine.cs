using EventBus;
using TimeLine.EventBus.Events.Grid;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class SelectFieldLine : MonoBehaviour
    {
        [SerializeField] Color selectedColor;
        [SerializeField] Color deselectedColor;
        [Space]
        [SerializeField] private Image image;
        [Space]
        [SerializeField] private AnimationFieldLine animationField;
        
        private GameEventBus _eventBus;

        [Inject]
        private void Construct(GameEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Select()
        {
            image.color = selectedColor;
            _eventBus.Raise(new SelectFieldLineEvent(animationField.FieldLineData, UnityEngine.Input.GetKey(KeyCode.LeftShift)));
        }

        public void Deselect()
        {
            image.color = deselectedColor;
        }
    }
}
