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
        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.L))
            {
                Remove(keyframeVizualizer.SelectedKeyframe.Track, keyframeVizualizer.SelectedKeyframe.Keyframe);
            }
        }

        void Remove(Track track, Keyframe.Keyframe keyframe)
        {
            track.RemoveKeyframe(keyframe);
            _gameEventBus.Raise(new RemoveKeyframeEvent());
        }
    }
}