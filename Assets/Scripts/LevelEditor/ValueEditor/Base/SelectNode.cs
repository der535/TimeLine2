using EventBus;
using TimeLine.EventBus.Events.ValueEditor;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor
{
    public class SelectNode : MonoBehaviour
    {
        [SerializeField] private RectTransform select;
        [SerializeField] private Node _node;

        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        public void SetSelected(bool selected)
        {
            select.gameObject.SetActive(selected);
        }

        public void Select()
        {
            _gameEventBus.Raise(new SelectNodeEvent(_node));
        }
    }
}