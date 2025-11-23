using System.Collections.Generic;
using System.Linq;
using EventBus;
using Newtonsoft.Json;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class KeyframeCopy : MonoBehaviour
    {
        [SerializeField] private KeyfeameVizualizer keyframeVizualizer;
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private KeyframeTrackStorage trackStorage;
        [SerializeField] private WindowsFocus windowsFocus;
        [SerializeField] private KeyframeTypeActive keyframeTypeActive;
        [SerializeField] private BezierSelectPointsController bezierSelectPointsController;

        private Main _main;
        private ActionMap _actionMap;
        private GameEventBus _gameEventBus;

        private List<(KeyframeSaveData, Track)> copyKeyframes = new();
        private double minTime;

        [Inject]
        private void Construct(Main main, GameEventBus eventBus, ActionMap actionMap)
        {
            _gameEventBus = eventBus;
            _main = main;
            _actionMap = actionMap;
        }

        private void Start()
        {
            _actionMap.Editor.C.started += _ =>
            {
                if (!_actionMap.Editor.LeftCtrl.IsPressed() || !windowsFocus.IsFocused) return;

                if (!keyframeTypeActive.IsBezier() && keyframeVizualizer.SelectedKeyframe.Count > 0)
                {
                    copyKeyframes.Clear();
                    
                    minTime = keyframeVizualizer.GetMinTimeSelectedKeyframe(keyframeVizualizer.SelectedKeyframe);
                    foreach (var keyframe in keyframeVizualizer.SelectedKeyframe)
                    {
                        Copy(keyframe.Keyframe, keyframe.Track);
                    }
                }
                else if(bezierSelectPointsController.selectedPoints.Count > 0)
                {
                    copyKeyframes.Clear();

                    keyframeVizualizer.GetAllKeyframes();

                    var result = keyframeVizualizer.GetAllKeyframes().Where(k =>
                        bezierSelectPointsController.selectedPoints.Any(b => k.Item2 == b.BezierDragPoint._keyframe));
                    
                    var valueTuples = result.ToList();
                    minTime = valueTuples.Min(i => i.Item2.Ticks);
                    
                    foreach (var keyframe in valueTuples)
                    {
                        Copy(keyframe.Item2, keyframe.Item1);
                    }
                }
                // Проверка на активное состояние вкладки и состояние кривых или ключевых кадров
            };

            _actionMap.Editor.V.started += _ =>
            {
                if (_actionMap.Editor.LeftCtrl.IsPressed() &&
                    windowsFocus.IsFocused &&
                    trackObjectStorage.selectedObject != null &&
                    copyKeyframes != null &&
                    copyKeyframes.Count > 0)
                {
                    foreach (var keyframe in copyKeyframes)
                    {
                        Paste(keyframe.Item1, keyframe.Item2, trackObjectStorage.selectedObject.trackObject, minTime);
                    }
                }
            };
        }

        private void Copy(Keyframe.Keyframe keyframe, Track track)
        {
            var data = keyframe.ToSaveData();
            copyKeyframes.Add((data, track));
        }

        private void Paste(KeyframeSaveData keyframe, Track track, TrackObject trackObject, double minTimeSelected)
        {
            Keyframe.Keyframe loadedKeyframe = Keyframe.Keyframe.FromSaveData(keyframe);
            var difference = keyframe.Ticks - minTimeSelected;
            loadedKeyframe.Ticks = _main.TicksCurrentTime() - trackObject.StartTimeInTicks + difference;
            track.AddKeyframe(loadedKeyframe);
            _gameEventBus.Raise(new AddKeyframeEvent(loadedKeyframe));
        }
    }
}