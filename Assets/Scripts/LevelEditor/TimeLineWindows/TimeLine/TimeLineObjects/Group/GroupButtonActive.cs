using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class GroupButtonActive : MonoBehaviour
    {
        [SerializeField] private Button createGroupButton;
        
        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        private readonly EventBinder _binder = new();

        private void Awake()
        {
            createGroupButton.interactable = false;
            // Цепочка подписок (Fluent API)
            _binder
                .Add(_gameEventBus, (ref SelectObjectEvent _) => createGroupButton.interactable = true)
                .Add(_gameEventBus, (ref DeselectObjectEvent e) => createGroupButton.interactable = e.SelectedObjects.Count > 0)
                .Add(_gameEventBus, (ref DeselectAllObjectEvent _) => createGroupButton.interactable = false);
        }

        private void OnDestroy()
        {
            // Очистка всех подписок разом
            _binder.Dispose();
        }
    }
}
