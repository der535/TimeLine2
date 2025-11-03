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
        private ActionMap _actionMap;
        private GameEventBus _gameEventBus;
        
        [Inject]
        private void Construct(Main main, GameEventBus eventBus, ActionMap actionMap)
        {
            _gameEventBus = eventBus;
            _main = main;
            _actionMap = actionMap;
        }

        private void Start()
        {
            _actionMap.Editor.F.started += _ =>
            {
                foreach (var keyframe in _keyframeVizualizer.SelectedKeyframe)
                {
                    Copy(keyframe.Keyframe, keyframe.Track,
                        _trackObjectStorage.selectedObject.trackObject, _keyframeVizualizer.GetMinTimeSelectedKeyframe()); //todo Исправить
                }
            }; 
        }

        private void Copy(Keyframe.Keyframe keyframe, Track track, TrackObject trackObject, double minTimeSelected)
        {
            var difference = keyframe.Ticks - minTimeSelected;
            Keyframe.Keyframe copyKeyframe = keyframe.Clone();
            copyKeyframe.Ticks = _main.TicksCurrentTime() - trackObject.StartTimeInTicks+difference;
            track.AddKeyframe(copyKeyframe);
            _gameEventBus.Raise(new AddKeyframeEvent(copyKeyframe));
        }
    }
}
