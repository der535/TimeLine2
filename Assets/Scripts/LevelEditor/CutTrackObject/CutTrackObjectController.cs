using System;
using System.Collections.Generic;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using Unity.Mathematics;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.CutTrackObject
{
    public class CutTrackObjectController : MonoBehaviour
    {
        [SerializeField] private WindowsFocus _windowsFocus;
        SelectObjectController _selectObjectController;
        ActionMap _actionMap;
        M_PlaybackState _playbackState;
        TrackObjectClipboard _clipboard;

        [Inject]
        private void Construct(SelectObjectController selectObjectController, ActionMap actionMap,
            M_PlaybackState playbackState, TrackObjectClipboard clipboard)
        {
            _selectObjectController = selectObjectController;
            _actionMap = actionMap;
            _playbackState = playbackState;
            _clipboard = clipboard;
        }

        private void Start()
        {
            _actionMap.Editor.CutRight.started += context =>
            {
                if(!_windowsFocus.IsFocused) return;
                
                foreach (var trackObject in _selectObjectController.SelectObjects)
                {
                    var endPosition = trackObject.components.Data.TimeDurationInTicks +
                                      trackObject.components.Data.StartTimeInTicks;
                    var delta = endPosition - _playbackState.SmoothTimeInTicks;
                    var newDuraction = trackObject.components.Data.TimeDurationInTicks -
                                       delta;

                    if (trackObject is TrackObjectGroup _)
                    {
                        var fullDuration = trackObject.components.Data.TimeDurationInTicks +
                                           math.abs(trackObject.components.Data.ReducedRight);
                        newDuraction = math.clamp(newDuraction, 1, fullDuration);
                        ;
                        var rightReduceNew = fullDuration - newDuraction;

                        trackObject.components.TrackObject.RightResize(newDuraction);
                        trackObject.components.Data.ReducedRight = -rightReduceNew;
                        //Хуяк ебать написал настрочунькал буковки неугодные
                        //Насчитал циферки злодеятельные
                    }
                    else
                    {
                        trackObject.components.TrackObject.RightResize(newDuraction);
                    }
                }
            };

            _actionMap.Editor.CutLeft.started += context =>
            {
                if(!_windowsFocus.IsFocused) return;
                
                foreach (var trackObject in _selectObjectController.SelectObjects)
                {
                    if (trackObject is TrackObjectGroup _)
                    {
                        var realStartPosition = trackObject.components.Data.StartTimeInTicks +
                                                trackObject.components.Data.ReducedLeft;
                        Debug.Log(realStartPosition);
                        var realDuraction = trackObject.components.Data.TimeDurationInTicks +
                                            math.abs(trackObject.components.Data.ReducedLeft);
                        Debug.Log(realDuraction);
                        Debug.Log($"min {realStartPosition}");
                        Debug.Log($"min {realStartPosition + realDuraction - 1}");
                        var setNewTimeClamped = math.clamp(_playbackState.SmoothTimeInTicks, realStartPosition,
                            realStartPosition + realDuraction - 1);
                        Debug.Log(setNewTimeClamped);

                        trackObject.components.TrackObject.LeftResize(setNewTimeClamped);
                        trackObject.components.Data.ReducedLeft = -setNewTimeClamped + realStartPosition;
                    }
                    else
                    {
                        trackObject.components.TrackObject.LeftResize(_playbackState.SmoothTimeInTicks);
                    }
                }
            };

            _actionMap.Editor.CutHalf.started += context =>
            {
                if (!_actionMap.Editor.LeftCtrl.IsPressed())
                {
                    var targetObjects = new List<TrackObjectPacket>(_selectObjectController.SelectObjects);
                    foreach (var trackObject in targetObjects.ToArray())
                    {
                        if (_playbackState.SmoothTimeInTicks > trackObject.components.Data.StartTimeInTicks &&
                            _playbackState.SmoothTimeInTicks < trackObject.components.Data.StartTimeInTicks +
                            trackObject.components.Data.TimeDurationInTicks)
                        {
                        }
                        else
                        {
                            targetObjects.Remove(trackObject);
                        }
                    }


                    var copyData = _clipboard.CopyObjects(targetObjects);

                    foreach (var trackObject in _selectObjectController.SelectObjects)
                    {
                        var endPosition = trackObject.components.Data.TimeDurationInTicks +
                                          trackObject.components.Data.StartTimeInTicks;
                        var delta = endPosition - _playbackState.SmoothTimeInTicks;
                        var newDuraction = trackObject.components.Data.TimeDurationInTicks -
                                           delta;

                        if (trackObject is TrackObjectGroup _)
                        {
                            var fullDuration = trackObject.components.Data.TimeDurationInTicks +
                                               math.abs(trackObject.components.Data.ReducedRight);
                            newDuraction = math.clamp(newDuraction, 1, fullDuration);
                            ;
                            var rightReduceNew = fullDuration - newDuraction;

                            trackObject.components.TrackObject.RightResize(newDuraction);
                            trackObject.components.Data.ReducedRight = -rightReduceNew;
                            //Хуяк ебать написал настрочунькал буковки неугодные
                            //Насчитал циферки злодеятельные
                        }
                        else
                        {
                            trackObject.components.TrackObject.RightResize(newDuraction);
                        }
                    }
                    


                    _clipboard.PasteObjectsFromSave(copyData, 0, (List<TrackObjectPacket> list) =>
                    {
                        foreach (var trackObject in list)
                        {
                            if (trackObject is TrackObjectGroup _)
                            {
                                var realStartPosition = trackObject.components.Data.StartTimeInTicks +
                                                        trackObject.components.Data.ReducedLeft;
                                Debug.Log(realStartPosition);
                                var realDuraction = trackObject.components.Data.TimeDurationInTicks +
                                                    math.abs(trackObject.components.Data.ReducedLeft);
                                Debug.Log(realDuraction);
                                Debug.Log($"min {realStartPosition}");
                                Debug.Log($"min {realStartPosition + realDuraction - 1}");
                                var setNewTimeClamped = math.clamp(_playbackState.SmoothTimeInTicks, realStartPosition,
                                    realStartPosition + realDuraction - 1);
                                Debug.Log(setNewTimeClamped);

                                trackObject.components.TrackObject.LeftResize(setNewTimeClamped);
                                trackObject.components.Data.ReducedLeft = -setNewTimeClamped + realStartPosition;
                            }
                            else
                            {
                                trackObject.components.TrackObject.LeftResize(_playbackState.SmoothTimeInTicks);
                            }
                        }
                    },true);
                    
                    
                    
                }
            };
        }
    }
}