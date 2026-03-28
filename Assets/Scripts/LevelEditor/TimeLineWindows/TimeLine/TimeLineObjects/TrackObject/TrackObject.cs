using System;
using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.EventBus.Events.Misc;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using TimeLine.Keyframe;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject
{
    public class TrackObject : ITickable, IDisposable
    {
        private ITrackObjectView _trackObjectView;
        private TrackObjectState _state;
        private TrackObjectData _data;

        private readonly int _deathZone = 5;

        private TimeLineSettings _timeLineSettings;
        private TrackObjectStorage _trackObjectStorage;
        private TrackStorage _trackStorage;
        private TimeLineConverter _timeLineConverter;
        private MainObjects _mainObjects;
        private GameEventBus _gameEventBus;
        private TimeLineScroll _timeLineScroll;

        private GridUI _gridUI;

        private KeyframeTrackStorage _keyframeTrackStorage;
        private SelectObjectController _selectObjectController;

        internal Action<double> Rezise { get; set; }

        private ActionMap _actionMap;

        [Inject]
        private void Construct(
            TimeLineSettings timeLineSettings,
            TrackObjectStorage trackObjectStorage,
            TrackStorage trackStorage,
            TimeLineConverter timeLineConverter,
            MainObjects mainObjects,
            GameEventBus gameEventBus,
            TimeLineScroll timeLineScroll,
            GridUI gridUI,
            Main main,
            KeyframeTrackStorage keyframeTrackStorage,
            SelectObjectController selectObjectController,
            ActionMap actionMap)
        {
            _gridUI = gridUI;
            _timeLineSettings = timeLineSettings;
            _trackObjectStorage = trackObjectStorage;
            _trackStorage = trackStorage;
            _timeLineConverter = timeLineConverter;
            _mainObjects = mainObjects;
            _gameEventBus = gameEventBus;
            _timeLineScroll = timeLineScroll;
            _keyframeTrackStorage = keyframeTrackStorage;
            _selectObjectController = selectObjectController;
        }

        internal void Setup(TrackObjectData trackObjectData, ITrackObjectView View, TrackObjectState state)
        {
            _gameEventBus.SubscribeTo<ScrollTimeLineEvent>(ScrollTimeLineEvent);
            _gameEventBus.SubscribeTo<TimeLineZoomEvent>(TimeLineZoomEvent);
            _gameEventBus.SubscribeTo<OpenEditorEvent>(OpenEditorEvent);

            _data = trackObjectData;
            _state = state;
            _trackObjectView = View;
            _trackObjectView.Rename(trackObjectData.Name);
            _data.OnChangeDuration += _ => UpdateVisuals();

            UpdateVisuals();
        }

        private void ScrollTimeLineEvent(ref ScrollTimeLineEvent data) => UpdateVisuals();
        private void TimeLineZoomEvent(ref TimeLineZoomEvent data) => UpdateVisuals();
        private void OpenEditorEvent(ref OpenEditorEvent data) => UpdateVisuals();

        public void Dispose()
        {
            _gameEventBus.UnsubscribeFrom<ScrollTimeLineEvent>(ScrollTimeLineEvent);
            _gameEventBus.UnsubscribeFrom<TimeLineZoomEvent>(TimeLineZoomEvent);
            _gameEventBus.UnsubscribeFrom<OpenEditorEvent>(OpenEditorEvent);
            _trackObjectView?.Destroy();
        }


        private Vector2 GetMousePosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _mainObjects.CanvasRectTransform,
                Mouse.current.position.ReadValue(),
                _mainObjects.MainCamera,
                out var currentLocalPosition);
            return currentLocalPosition;
        }


        public void SaveResizingData()
        {
            _state.StartResizingDuractionInTicks = _data.TimeDurationInTicks;
            _state.StartResizingTimeInTicks = _data.StartTimeInTicks;
        }

        public void MultipleRightResize(double deltaTicks)
        {
            _data.ChangeDurationInTicks(
                Math.Max(RoundTicksToGrid(_state.StartResizingDuractionInTicks + deltaTicks), 1));
        }

        public void MultipleLeftResize(double deltaTicks)
        {
            double newStartTimeInTicks = _state.StartResizingTimeInTicks - deltaTicks;
            double newDurationInTicks = _state.StartResizingDuractionInTicks + deltaTicks;

            newStartTimeInTicks = RoundTicksToGrid(newStartTimeInTicks);
            newDurationInTicks = RoundTicksToGrid(newDurationInTicks);

            _data.StartTimeInTicks = newStartTimeInTicks;
            _data.ChangeDurationInTicks(newDurationInTicks);
        }


        public void Tick()
        {
            Drag();

            if (_state.IsResizing)
            {
                float pixelsPerBeat = _timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Zoom;

                if (_state.IsRightResizing)
                {
                    Vector2 currentMousePosition = GetMousePosition();
                    float deltaPixels = currentMousePosition.x - _state.StartMousePosition.x;
                    double deltaTicks = (deltaPixels / pixelsPerBeat) * TimeLineConverter.TICKS_PER_BEAT;
                    _state.DeltaticksRight = deltaTicks;

                    double proposedDuration = _state.StartResizingDuractionInTicks + deltaTicks;
                    double roundedProposedDuration = RoundTicksToGrid(proposedDuration);
                    double roundedChange = roundedProposedDuration - _state.StartResizingDuractionInTicks;

                    // ✅ Проверяем флаг: если лимиты отключены — пропускаем проверку
                    if (_data.EnableResizeLimits && _data.ReducedRight + roundedChange > 0)
                    {
                        double maxAllowedChange = -_data.ReducedRight;
                        double clampedDuration = _state.StartResizingDuractionInTicks + maxAllowedChange;
                        double roundedClampedDuration = RoundTicksToGrid(clampedDuration);

                        _selectObjectController.MultipleResizingRight(this, maxAllowedChange);
                        _data.ChangeDurationInTicks(Math.Max(roundedClampedDuration, 1));
                        return;
                    }

                    _selectObjectController.MultipleResizingRight(this, deltaTicks);

                    _data.ChangeDurationInTicks(Math.Max(roundedProposedDuration, 1));
                }
                else
                {
                    Vector2 currentMousePosition = GetMousePosition();
                    float deltaPixels = _state.StartMousePosition.x - currentMousePosition.x;
                    double deltaTicks = (deltaPixels / pixelsPerBeat) * TimeLineConverter.TICKS_PER_BEAT;
                    _state.DeltaticksLeft = deltaTicks;

                    double proposedDuration = _state.StartResizingDuractionInTicks + deltaTicks;
                    double roundedProposedDuration = RoundTicksToGrid(proposedDuration);
                    double roundedChange = roundedProposedDuration - _state.StartResizingDuractionInTicks;

                    // ✅ Проверяем флаг: если лимиты отключены — пропускаем проверку
                    if (_data.EnableResizeLimits && _data.ReducedLeft + roundedChange > 0)
                    {
                        double maxAllowedChange = -_data.ReducedLeft;
                        double clampedDuration = _state.StartResizingDuractionInTicks + maxAllowedChange;
                        double clampedStartTime = _state.StartResizingTimeInTicks - maxAllowedChange;

                        double roundedClampedDuration = RoundTicksToGrid(clampedDuration);
                        double roundedClampedStartTime = RoundTicksToGrid(clampedStartTime);

                        _selectObjectController.MultipleResizingLeft(this, maxAllowedChange);
                        Rezise?.Invoke(roundedClampedStartTime - _data.StartTimeInTicks);
                        _data.StartTimeInTicks = roundedClampedStartTime;
                        _data.ChangeDurationInTicks(roundedClampedDuration);
                        return;
                    }

                    _selectObjectController.MultipleResizingLeft(this, deltaTicks);
                    double newStartTimeInTicks = _state.StartResizingTimeInTicks - deltaTicks;
                    double newDurationInTicks = _state.StartResizingDuractionInTicks + deltaTicks;

                    newStartTimeInTicks = RoundTicksToGrid(newStartTimeInTicks);
                    newDurationInTicks = RoundTicksToGrid(newDurationInTicks);

                    Rezise?.Invoke(newStartTimeInTicks - _data.StartTimeInTicks);
                    _data.StartTimeInTicks = newStartTimeInTicks;
                    _data.ChangeDurationInTicks(newDurationInTicks);
                }
            }
            else if (_state.WasResizing)
            {
                _state.WasResizing = false;
            }
        }

        private void UpdateVisuals()
        {
            if (_trackObjectView == null) return;
            float durationInBeats = (float)(_data.TimeDurationInTicks / TimeLineConverter.TICKS_PER_BEAT);
            _trackObjectView.SetSizeDelta(new Vector2(
                durationInBeats * (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Zoom),
                _trackObjectView.GetSizeDelta().y));

            CalculatePosition();
        }

        internal void CalculatePosition()
        {
            float startTimeInSeconds = (float)TimeLineConverter.Instance.TicksToSeconds(_data.StartTimeInTicks);
            _trackObjectView.SetAnchorPosition(new Vector2(
                _timeLineConverter.GetAnchorPositionFromTime(startTimeInSeconds) +
                _trackObjectView.GetSizeDelta().x / 2,
                _trackObjectView.GetAnchorPosition().y));
        }

        public void SavePosition()
        {
            _state.StartTrackObjectTicks = _data.StartTimeInTicks;
        }

        private void Drag()
        {
            if (!_state.IsDragging) return;


            //Проверка если transform.parent у трек обжекта (то есть на какой линии в редакторе находится) не равняется той которая записана в данных то меняем парент линии
            if (_trackObjectView.GetParent() != _trackStorage.GetTrackLineByIndex(_data.TrackLineIndex).RectTransform)
            {
                _trackObjectView.SetParent(_trackStorage.GetTrackLineByIndex(_data.TrackLineIndex).RectTransform);
            }

            _trackObjectView.SetOffsetMax(new Vector2(_trackObjectView.GetOffsetMax().x, 0));
            _trackObjectView.SetOffsetMin(new Vector2(_trackObjectView.GetOffsetMin().x, 0));

            UpdatePosition();
            CalculatePosition();
        }

        internal void UpdatePosition()
        {
            Vector2 currentMousePos = GetMousePosition();
            float currentMouseXLocal = currentMousePos.x;
            float mouseDeltaXLocal = currentMouseXLocal - _state.StartMouseXLocal;
            double deltaTicks = AnchorPositionDeltaToTicks(mouseDeltaXLocal);
            if (Mathf.Abs(mouseDeltaXLocal) < _deathZone && _state.DeathZonePass == false)
            {
                return;
            }
            else
            {
                _state.DeathZonePass = true;
            }

            int oldIndex = _data.TrackLineIndex;
            _data.TrackLineIndex = _trackStorage.CheckTracks(_data.TrackLineIndex);
            int newIndex = _data.TrackLineIndex;
            if (oldIndex != -1 && newIndex != -1 && Mathf.Abs(oldIndex - newIndex) > 0)
                _selectObjectController.MultipleChangeTrackLine(this, oldIndex - newIndex);

            _selectObjectController.MultipleMove(this, deltaTicks);
            _data.StartTimeInTicks = RoundTicksToGrid(_state.StartTrackObjectTicks + deltaTicks);
            _trackObjectStorage.UpdatePositionSelectedTrackObject();

            double AnchorPositionDeltaToTicks(float deltaAnchorPosition)
            {
                float pixelsPerBeat = _timeLineScroll.Zoom;
                float beatsDelta = deltaAnchorPosition / pixelsPerBeat;
                return beatsDelta * TimeLineConverter.TICKS_PER_BEAT;
            }
        }

        internal void AddTicksMove(double deltaTicks)
        {
            _data.StartTimeInTicks = RoundTicksToGrid(_state.StartTrackObjectTicks + deltaTicks);
            _trackObjectStorage.UpdatePositionSelectedTrackObject();
            CalculatePosition();
        }

        internal void AddLineTrackIndex(int addedIndex)
        {
            _data.TrackLineIndex -= addedIndex;
            _trackObjectView.SetParent(_trackStorage.GetTrackLineByIndex(_data.TrackLineIndex).RectTransform);
            _trackObjectView.SetAnchorPosition(new Vector2(_trackObjectView.GetAnchorPosition().x, 0));
        }


        /// <summary>
        /// Огругляет тики по сетке выставленной игроком
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        private double RoundTicksToGrid(double ticks)
        {
            return _gridUI.RoundTicksToGrid(ticks);
        }
    }
}