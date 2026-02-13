using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine.KeyframeSelect;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeType;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.ValueEditor.NodeLogic;
using TimeLine.LevelEditor.ValueEditor.Save;
using TimeLine.LevelEditor.ValueEditor.Test;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class KeyframeCopy : MonoBehaviour
    {
        [SerializeField] private KeyframeVizualizer keyframeVizualizer;

        [FormerlySerializedAs("keyframeSelectStorage")] [SerializeField]
        private KeyframeSelectController keyframeSelectController;

        [SerializeField] private KeyframeTrackStorage trackStorage;
        [SerializeField] private WindowsFocus windowsFocus;

        [SerializeField] private BezierSelectPointsController bezierSelectPointsController;

        private TrackObjectStorage _trackObjectStorage;
        private ActionMap _actionMap;
        private GameEventBus _gameEventBus;
        private KeyframeSelectedStorage _selectedKeyframesStorage;
        private KeyframeVizualizer _selectedKeyframeVizualizer;
        private M_KeyframeActiveTypeData _mKeyframeActiveTypeData;
        private SaveNodes _saveNodes;
        private SelectObjectController _selectObjectController;

        private List<(KeyframeSaveData, Track)> copyKeyframes = new();
        private double minTime;

        [Inject]
        private void Construct(Main main, GameEventBus eventBus, ActionMap actionMap,
            KeyframeSelectedStorage selectedKeyframesStorage, KeyframeVizualizer keyframeVizualizer,
            M_KeyframeActiveTypeData mKeyframeActiveTypeData, SaveNodes saveNodes, TrackObjectStorage trackObjectStorage, SelectObjectController selectObjectController)
        {
            _mKeyframeActiveTypeData = mKeyframeActiveTypeData;
            _gameEventBus = eventBus;
            _actionMap = actionMap;
            _selectedKeyframesStorage = selectedKeyframesStorage;
            _selectedKeyframeVizualizer = keyframeVizualizer;
            _saveNodes = saveNodes;
            _trackObjectStorage = trackObjectStorage;
            _selectObjectController = selectObjectController;
        }

        private void Start()
        {
            _actionMap.Editor.C.started += _ =>
            {
                if (!_actionMap.Editor.LeftCtrl.IsPressed() || !windowsFocus.IsFocused) return;

                if (_mKeyframeActiveTypeData.ActiveType == M_KeyframeType.Keyframe && _selectedKeyframesStorage.Keyframes.Count > 0)
                {
                    copyKeyframes.Clear();

                    minTime = keyframeVizualizer.GetMinTimeSelectedKeyframe(_selectedKeyframesStorage.Keyframes);
                    foreach (var keyframe in _selectedKeyframesStorage.Keyframes)
                    {
                        Copy(keyframe, _selectedKeyframeVizualizer.GetKeyframeObjectData(keyframe).Track);
                    }
                }
                else if (_selectedKeyframesStorage.Keyframes.Count > 0)
                {
                    copyKeyframes.Clear();

                    keyframeVizualizer.GetAllKeyframes();

                    var result = keyframeVizualizer.GetAllKeyframes().Where(k =>
                        _selectedKeyframesStorage.Keyframes.Any(b => k.Item2 == b));

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
                    _trackObjectStorage.selectedObject != null &&
                    copyKeyframes != null &&
                    copyKeyframes.Count > 0)
                {
                    foreach (var keyframe in copyKeyframes)
                    {
                        Paste(keyframe.Item1, keyframe.Item2, _trackObjectStorage.selectedObject.trackObject, minTime);
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
            (OutputLogic item1, List<IInitializedNode> item2) = _saveNodes.LoadLogicOnly(keyframe.Graph, TypeToDataType.Convert(keyframe.DataType));
            Keyframe.Keyframe loadedKeyframe = Keyframe.Keyframe.FromSaveData(keyframe,item1,item2);
            var difference = keyframe.Ticks - minTimeSelected;
            loadedKeyframe.Ticks = TimeLineConverter.Instance.TicksCurrentTime() - trackObject.StartTimeInTicks +
                                   difference;
            track.AddKeyframe(loadedKeyframe);
            _gameEventBus.Raise(new AddKeyframeEvent(loadedKeyframe));
        }
    }
}