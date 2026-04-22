using System;
using EventBus;
using TimeLine.Cursor;
using TimeLine.EventBus.Events.Misc;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.Core;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject
{
    public class TrackObjectSelect : MonoBehaviour
    {
        private SelectObjectController _select;

        private TrackObjectState _state;
        private TrackObjectData _data;
        private ITrackObjectView _view;
        private TrackObject _trackObject;
        private MainObjects _mainObjects;
        private GameEventBus _eventBus;
        private SelectObjectController _selectController;
        private TrackObjectStorage _trackObjectStorage;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private GridUI _gridUI;
        private CursorController _cursorController;

        [Inject]
        private void Construct(SelectObjectController state, MainObjects mainObjects, GameEventBus eventBus,
            TrackObjectStorage trackObjectStorage,
            KeyframeTrackStorage keyframeTrackStorage, GridUI gridUI, CursorController cursorController)
        {
            _select = state;
            _eventBus = eventBus;
            _mainObjects = mainObjects;
            _trackObjectStorage = trackObjectStorage;
            _keyframeTrackStorage = keyframeTrackStorage;
            _gridUI = gridUI;
            _selectController = state;
            _cursorController = cursorController;
        }

        public void Setup(TrackObject trackObject, TrackObjectState state, TrackObjectData data, ITrackObjectView view)
        {
            _trackObject = trackObject;
            _state = state;
            _data = data;
            _view = view;
        }

        public void OnMouseDown()
        {
            _state.StartTrackObjectTicks = _data.StartTimeInTicks;
            Vector2 mousePos =
                MousePosition.GetMousePosition(_mainObjects.CanvasRectTransform, _mainObjects.MainCamera);
            _state.StartMouseXLocal = mousePos.x;
            _state.IsDragging = true;
            _state.DeathZonePass = false;
            _select.StartMultipleMove(null);
        }

        public void SetResizeRight(bool isResizing)
        {
            _state.StartResizingDuractionInTicks = _data.TimeDurationInTicks;
            _state.IsRightResizing = true;
            _state.IsResizing = isResizing;
            if (_state.IsResizing)
            {
                _state.StartMousePosition =
                    MousePosition.GetMousePosition(_mainObjects.CanvasRectTransform, _mainObjects.MainCamera);
            }
            else
            {
                if (_data.EnableResizeLimits)
                {
                    // _data.ReducedRight = Math.Round(Math.Min(_data.ReducedRight +
                    //                                          _gridUI.RoundTicksToGrid(
                    //                                              _state.StartResizingDuractionInTicks +
                    //                                              _state.DeltaticksRight) -
                    //                                          _state.StartResizingDuractionInTicks, 0));
                }
            }

            if (!isResizing && _state.WasResizing)
            {
                _state.WasResizing = false;
            }
            else if (isResizing)
            {
                _state.WasResizing = true;
            }

            _eventBus.Raise(new TrackObjectResizing(isResizing));
            _selectController.SaveResizingData(_trackObject);
        }

        public void SetResizeCursor(bool isResizing)
        {
            if(isResizing) _cursorController.SetResizeHorizontal();
            else _cursorController.SetIdel();
        }
        
        public void SetResizeLeft(bool isResizing)
        {
            _selectController.MultipleStopResizingLeft(_trackObject);
            _state.StartResizingDuractionInTicks = _data.TimeDurationInTicks;
            _state.StartResizingTimeInTicks = _data.StartTimeInTicks;
            _state.IsRightResizing = false;
            _eventBus.Raise(new TrackObjectResizing(isResizing));
            _state.IsResizing = isResizing;

            if (!isResizing)
            {
                ApplyKeyframeOffset();
                if (_data.EnableResizeLimits)
                {
                    // _data.ReducedLeft = Math.Round(Math.Min(_data.ReducedLeft +
                    //                                         _gridUI.RoundTicksToGrid(
                    //                                             _state.StartResizingDuractionInTicks +
                    //                                             _state.DeltaticksLeft) -
                    //                                         _state.StartResizingDuractionInTicks, 0));
                }
            }

            if (_state.IsResizing)
            {
                _state.StartMousePosition =
                    MousePosition.GetMousePosition(_mainObjects.CanvasRectTransform, _mainObjects.MainCamera);
                _state.InitialStartTimeInTicks = _data.StartTimeInTicks;
            }

            
            _selectController.SaveResizingData(_trackObject);
        }

        private void ApplyKeyframeOffset()
        {
            double offset = Math.Round(_data.StartTimeInTicks - _state.InitialStartTimeInTicks);
            if (offset != 0)
            {
                foreach (var node in _trackObjectStorage.GetTrackObjectData(_trackObject).branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.AddOffsetKeyframes(-offset);
                    }
                }
            }
        }
        
        public void Select()
        {
            _trackObjectStorage.SelectObjectTrackObject(_trackObject);
        }

        /// <summary>
        /// Устанавливает цвет выделения трек обжекту
        /// </summary>
        public void SelectColor()
        {
            _view.SetColor( Color.yellow);
        }

        /// <summary>
        /// Устанавливает цвет снятого выделения трек обжекту
        /// </summary>
        public void Deselect()
        {
            _view.SetColor(Color.gray);
        }

        

        public void OnMouseUp()
        {
            _state.IsDragging = false;
        }
    }
}