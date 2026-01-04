using System;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.Tabs.InspectorTab.Keyframe.KeyframeTimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class KeyframeRemover : MonoBehaviour
    {
        [FormerlySerializedAs("keyframeSelectStorage")] [SerializeField] private KeyframeSelectController keyframeSelectController;
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
                if (!windowsFocus.IsFocused || keyframeSelectController.SelectedKeyframe == null) return;

                foreach (var keyframe in keyframeSelectController.SelectedKeyframe)
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