using System;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class KeyframeRemover : MonoBehaviour
    {
        [SerializeField] private KeyfeameVizualizer keyframeVizualizer;
        [SerializeField] private WindowsFocus windowsFocus;
        private GameEventBus _gameEventBus;
        private ActionMap _actionMap;

        [Inject]
        private void Construct(GameEventBus gameEventBus, ActionMap actionMap)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
        }

        private void Start()
        {
            _actionMap.Editor.X.started += (_) =>
            {
                if(!windowsFocus.IsFocused || keyframeVizualizer.SelectedKeyframe == null) return;

                foreach (var keyframe in keyframeVizualizer.SelectedKeyframe)
                {
                    Remove(keyframe.Track, keyframe.Keyframe);
                }
               
            }; 
        }

        void Remove(Track track, Keyframe.Keyframe keyframe)
        {
            track.RemoveKeyframe(keyframe);
            _gameEventBus.Raise(new RemoveKeyframeEvent());
        }
    }
}