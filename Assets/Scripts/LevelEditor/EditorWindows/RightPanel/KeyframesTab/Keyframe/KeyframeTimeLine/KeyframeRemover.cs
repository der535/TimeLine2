using System;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine.KeyframeSelect;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class KeyframeRemover : MonoBehaviour
    {
        [FormerlySerializedAs("keyframeSelectStorage")] [SerializeField]
        private KeyframeSelectController keyframeSelectController;

        [SerializeField] private WindowsFocus windowsFocus;
        private GameEventBus _gameEventBus;
        private ActionMap _actionMap;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private KeyframeSelectedStorage _keyframeSelectedStorage;
        private KeyframeVizualizer _keyframeVizualizer;

        [Inject]
        private void Construct(GameEventBus gameEventBus, ActionMap actionMap,
            KeyframeTrackStorage keyframeTrackStorage, KeyframeSelectedStorage keyframeSelectedStorage, KeyframeVizualizer keyframeVizualizer)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
            _keyframeTrackStorage = keyframeTrackStorage;
            _keyframeVizualizer = keyframeVizualizer;
            _keyframeSelectedStorage = keyframeSelectedStorage;
        }

        private void Start()
        {
            _actionMap.Editor.X.started += (_) =>
            {
                if (!windowsFocus.IsFocused || _keyframeSelectedStorage.Keyframes == null) return;

                foreach (var keyframe in _keyframeSelectedStorage.Keyframes)
                {
                    Remove(_keyframeVizualizer.GetKeyframeObjectData(keyframe).Track, keyframe);
                }
            };
        }

        public void Remove(Track track, Keyframe.Keyframe keyframe)
        {
            track.RemoveKeyframe(keyframe);
            if (track.Keyframes.Count == 0)
            {
                _keyframeTrackStorage.RemoveTrackWithNode(track);
            }

            _gameEventBus.Raise(new RemoveKeyframeEvent());
        }
    }
}