using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class KeyframeCopy : MonoBehaviour
    {
        [SerializeField] private KeyfeameVizualizer _keyframeVizualizer;
        [SerializeField] private TrackObjectStorage _trackObjectStorage;
        [SerializeField] private KeyframeTrackStorage _trackStorage;

        private Main _main;
        private GameEventBus _gameEventBus;
        
        [Inject]
        private void Construct(Main main, GameEventBus eventBus)
        {
            _gameEventBus = eventBus;
            _main = main;
        }
        
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F))
            {
                Copy(_keyframeVizualizer.SelectedKeyframe.Keyframe, _keyframeVizualizer.SelectedKeyframe.Track, _trackObjectStorage.selectedObject.trackObject); 
            }
        }

        private void Copy(Keyframe.Keyframe keyframe, Track track, TrackObject trackObject)
        {
            Keyframe.Keyframe copyKeyframe = keyframe.Clone();
            copyKeyframe.Ticks = _main.TicksCurrentTime() - trackObject.StartTimeInTicks;
            track.AddKeyframe(copyKeyframe);
            _gameEventBus.Raise(new AddKeyframeEvent(copyKeyframe));
        }
    }
}
