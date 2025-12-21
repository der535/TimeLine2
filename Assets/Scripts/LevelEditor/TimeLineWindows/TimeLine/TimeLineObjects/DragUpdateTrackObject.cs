using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class DragUpdateTrackObject : MonoBehaviour
    {
        private TrackObjectStorage _trackObjectStorage;
        private GameEventBus _eventBus;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private Main _main;
        
        [Inject]
        private void Constructor(GameEventBus gameEventBus, Main main, TrackObjectStorage trackObjectStorage, KeyframeTrackStorage keyframeTrackStorage)
        {
            _eventBus = gameEventBus;
            _trackObjectStorage = trackObjectStorage;
            _keyframeTrackStorage = keyframeTrackStorage;
            _main = main;
        }
        
        void Start()
        {
            _eventBus.SubscribeTo((ref DragTrackObjectEvent trackObjectEvent) =>
            {
                _trackObjectStorage.CheckActiveTrackSingle(trackObjectEvent.Track);
                _keyframeTrackStorage.Evaluate(_main.TicksCurrentTime());
            });
            
        }
    }
}
