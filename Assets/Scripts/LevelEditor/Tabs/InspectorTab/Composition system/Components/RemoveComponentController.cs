using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine.Components
{
    public class RemoveComponentController : MonoBehaviour
    {
        private GameEventBus _eventBus;
        private InitializedComponentController _initializedComponent;

        [Inject]
        private void Construct(GameEventBus eventBus, InitializedComponentController initializedComponent)
        {
            _eventBus = eventBus;
            _initializedComponent = initializedComponent;
        }

        private void Start()
        {
            _eventBus.SubscribeTo((ref RemoveComponentEvent data) =>
            {
                if (data.Component is IInitializedComponent initializedComponent)
                    _initializedComponent.Remove(initializedComponent);
                Destroy(data.Component);
                print(data.Component);
            });
        }
    }
}