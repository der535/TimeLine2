using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
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
        private KeyframeTrackStorage _keyframeTrackStorage;

        [Inject]
        private void Construct(GameEventBus eventBus, InitializedComponentController initializedComponent,
            EntityComponentController entityComponentController, KeyframeTrackStorage keyframeTrackStorage)
        {
            _eventBus = eventBus;
            _initializedComponent = initializedComponent;
            _entityComponentController = entityComponentController;
            _keyframeTrackStorage = keyframeTrackStorage;
        }

        private void Start()
        {
            _eventBus.SubscribeTo((ref RemoveComponentEvent data) =>
            {
                Debug.Log(data.Component);
                Debug.Log(data.TrackObjectPacket.sceneObjectID);
                _entityComponentController.RemoveComponent(data.Component, _keyframeTrackStorage, data.TrackObjectPacket.entity);
            }, 1);
        }
    }
}