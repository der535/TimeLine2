using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace TimeLine.Components
{
    public class RemoveComponentController : MonoBehaviour
    {
        private GameEventBus _eventBus;
        private InitializedComponentController _initializedComponent;
        private EntityComponentController _entityComponentController;

        [Inject]
        private void Construct(GameEventBus eventBus, InitializedComponentController initializedComponent, EntityComponentController entityComponentController)
        {
            _eventBus = eventBus;
            _initializedComponent = initializedComponent;
            _entityComponentController = entityComponentController;
        }

        private void Start()
        {
            _eventBus.SubscribeTo((ref RemoveComponentEvent data) =>
            {
                _entityComponentController.RemoveComponent(data.Component, data.TrackObjectPacket.entity);
            }, 1);
        }
    }
}