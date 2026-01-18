using System;
using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using TimeLine.Keyframe;
using TimeLine.TimeLine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects
{
    public class TrackObject : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private RectTransform rect;
        [SerializeField] private TrackObjectCustomizationController customizationController;
        [SerializeField] private TrackObjectVisual trackObjectVisual;

        [FormerlySerializedAs("ticksDeathZone")] [SerializeField]
        private int deathZone;

        #region Вспомогательные переменные

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

        private bool _isDragging;
        private bool _isResizing;
        private bool _isRightResizing;
        private bool _wasResizing;

        private double _startResizingDuractionInTicks;
        private double _startResizingTimeInTicks;
        private double _initialStartTimeInTicks;

        private Vector2 _startMousePosition;
        private double _startTrackObjectTicks;
        private float _startMouseXLocal;

        private double _deltaticksRight;
        private double _deltaticksLeft;

        public TrackObject offsetObject;

        #endregion

        private double _startTimeInTicks;
        private double _timeDurationInTicks;

        private bool _deathZonePass;

        internal double StartTimeInTicks
        {
            get { return Math.Round(_startTimeInTicks); }
            private set
            {
                _startTimeInTicks = Math.Round(value);
                OnChangeStartTime?.Invoke(_startTimeInTicks);
            }
        }

        internal bool isActive = true;
        internal bool isTemp = false;
        internal TrackObjectVisual Visual => trackObjectVisual;
        public void SetTime(double time) => _startTimeInTicks = time;
        internal Action<double> OnChangeStartTime;
        internal string _parentID;

        internal double TimeDuractionInTicks
        {
            get => Math.Round(_timeDurationInTicks);
            private set => _timeDurationInTicks = Math.Round(value);
        }

        internal TrackLine TrackLine { get; private set; }
        internal string Name { get; private set; }
        internal Action<double> Rezise { get; set; }
        internal float BeatDuraction => (float)(TimeDuractionInTicks / TimeLineConverter.TICKS_PER_BEAT);

        internal RectTransform RectTransform => rect;

        private bool _lockSize;
        public double _reducedLeft;
        public double _reducedRight;
        private ActionMap _actionMap;

        // 🔑 Новый флаг: включать/выключать ограничения ресайза
        private bool _enableResizeLimits = true;

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

        internal void Awake()
        {
            _gameEventBus.SubscribeTo<ScrollTimeLineEvent>(OnScroll);
            _gameEventBus.SubscribeTo<PanEvent>(OnScrollPan);
            _gameEventBus.SubscribeTo((ref OpenEditorEvent _) => UpdateVisuals());
        }

        private void OnDestroy()
        {
            _gameEventBus.UnsubscribeFrom<ScrollTimeLineEvent>(OnScroll);
            _gameEventBus.UnsubscribeFrom<PanEvent>(OnScrollPan);
        }

        internal bool GetActive() => rect.gameObject.activeSelf;
        internal TrackObjectCustomizationController CustomizationController() => customizationController;

        internal void Hide()
        {
            rect.gameObject.SetActive(false);
        }

        internal void Show()
        {
            rect.gameObject.SetActive(true);
        }

        internal void Rename(string name)
        {
            nameText.text = name;
        }

        internal void UpdateDuraction(double newDuractionInTicks)
        {
            var delta = newDuractionInTicks - (TimeDuractionInTicks - _reducedRight - _reducedLeft);
            _reducedRight -= delta;
            // print(_reducedRight);
        }

        // 🆕 Перегрузка с флагом
        internal void Setup(double ticksLifeTime, string name, TrackLine trackLine, string parentID,
            double startTimeInTicks, double reducedLeft, double reducedRight,
            bool enableResizeLimits = false)
        {
            StartTimeInTicks = startTimeInTicks;
            TrackLine = trackLine;
            TimeDuractionInTicks = ticksLifeTime;
            Name = name;
            nameText.text = name;
            _parentID = parentID;

            // _reducedRight = 0;
            // _reducedLeft = 0;
            
            _reducedLeft = reducedLeft;
            _reducedRight = reducedRight;
            _enableResizeLimits = enableResizeLimits; // ← Сохраняем флаг

            UpdateVisuals();
        }


        internal void GroupOffset(double tickOffset)
        {
            StartTimeInTicks -= tickOffset;
        }

        internal void GroupOffsetTrack(TrackObject track)
        {
            offsetObject = track;
        }

        internal double GetKeyframeTrackOffset()
        {
            var current = offsetObject;
            int depth = 0;
            const int maxDepth = 50; // защита от зависания

            while (current != null && depth < maxDepth)
            {
                // print($"offsetObject[{depth}] = {current}");
                current = current.offsetObject;
                depth++;
            }

            if (depth == maxDepth)
                print("Предупреждение: достигнут лимит глубины — возможна циклическая ссылка.");

            return StartTimeInTicks + (offsetObject != null ? offsetObject.GetKeyframeTrackOffset() : 0);
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

        public void SetResizeRight(bool isResizing)
        {
            _startResizingDuractionInTicks = TimeDuractionInTicks;
            _isRightResizing = true;
            _isResizing = isResizing;
            if (_isResizing)
            {
                _startMousePosition = GetMousePosition();
            }
            else
            {
                if (_enableResizeLimits)
                {
                    _reducedRight = Math.Round(Math.Min(_reducedRight +
                                             RoundTicksToGrid(_startResizingDuractionInTicks + _deltaticksRight) -
                                             _startResizingDuractionInTicks, 0));
                }
            }

            if (!isResizing && _wasResizing)
            {
                _wasResizing = false;
            }
            else if (isResizing)
            {
                _wasResizing = true;
            }

            _selectObjectController.SaveResizingData(this);
        }

        public void SaveResizingData()
        {
            _startResizingDuractionInTicks = TimeDuractionInTicks;
            _startResizingTimeInTicks = StartTimeInTicks;
        }

        public void MultipleRightResize(double deltaTicks)
        {
            ChangeDurationInTicks(RoundTicksToGrid(_startResizingDuractionInTicks + deltaTicks));
        }

        public void MultipleLeftResize(double deltaTicks)
        {
            double newStartTimeInTicks = _startResizingTimeInTicks - deltaTicks;
            double newDurationInTicks = _startResizingDuractionInTicks + deltaTicks;

            newStartTimeInTicks = RoundTicksToGrid(newStartTimeInTicks);
            newDurationInTicks = RoundTicksToGrid(newDurationInTicks);

            StartTimeInTicks = newStartTimeInTicks;
            ChangeDurationInTicks(newDurationInTicks);
        }

        public void SetResizeLeft(bool isResizing)
        {
            _startResizingDuractionInTicks = TimeDuractionInTicks;
            _startResizingTimeInTicks = StartTimeInTicks;
            _isRightResizing = false;
            _isResizing = isResizing;

            if (!isResizing)
            {
                ApplyKeyframeOffset();
                if (_enableResizeLimits)
                {
                    _reducedLeft = Math.Round(Math.Min(_reducedLeft +
                                            RoundTicksToGrid(_startResizingDuractionInTicks + _deltaticksLeft) -
                                            _startResizingDuractionInTicks, 0));
                }
            }

            if (_isResizing)
            {
                _startMousePosition = GetMousePosition();
                _initialStartTimeInTicks = StartTimeInTicks;
            }

            _selectObjectController.SaveResizingData(this);
        }

        private void ApplyKeyframeOffset()
        {
            double offset = Math.Round(StartTimeInTicks - _initialStartTimeInTicks);
            if (offset != 0)
            {
                foreach (var node in _trackObjectStorage.GetTrackObjectData(this).branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.AddOffsetKeyframes(-offset);
                    }
                }
            }
        }

        private void Update()
        {
            Drag();

            if (_isResizing)
            {
                float pixelsPerBeat = _timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Zoom;

                if (_isRightResizing)
                {
                    Vector2 currentMousePosition = GetMousePosition();
                    float deltaPixels = currentMousePosition.x - _startMousePosition.x;
                    double deltaTicks = (deltaPixels / pixelsPerBeat) * TimeLineConverter.TICKS_PER_BEAT;
                    _deltaticksRight = deltaTicks;

                    double proposedDuration = _startResizingDuractionInTicks + deltaTicks;
                    double roundedProposedDuration = RoundTicksToGrid(proposedDuration);
                    double roundedChange = roundedProposedDuration - _startResizingDuractionInTicks;

                    // ✅ Проверяем флаг: если лимиты отключены — пропускаем проверку
                    if (_enableResizeLimits && _reducedRight + roundedChange > 0)
                    {
                        double maxAllowedChange = -_reducedRight;
                        double clampedDuration = _startResizingDuractionInTicks + maxAllowedChange;
                        double roundedClampedDuration = RoundTicksToGrid(clampedDuration);

                        _selectObjectController.MultipleResizingRight(this, maxAllowedChange);
                        ChangeDurationInTicks(roundedClampedDuration);
                        return;
                    }

                    _selectObjectController.MultipleResizingRight(this, deltaTicks);
                    ChangeDurationInTicks(roundedProposedDuration);
                }
                else
                {
                    Vector2 currentMousePosition = GetMousePosition();
                    float deltaPixels = _startMousePosition.x - currentMousePosition.x;
                    double deltaTicks = (deltaPixels / pixelsPerBeat) * TimeLineConverter.TICKS_PER_BEAT;
                    _deltaticksLeft = deltaTicks;

                    double proposedDuration = _startResizingDuractionInTicks + deltaTicks;
                    double roundedProposedDuration = RoundTicksToGrid(proposedDuration);
                    double roundedChange = roundedProposedDuration - _startResizingDuractionInTicks;

                    // ✅ Проверяем флаг: если лимиты отключены — пропускаем проверку
                    if (_enableResizeLimits && _reducedLeft + roundedChange > 0)
                    {
                        double maxAllowedChange = -_reducedLeft;
                        double clampedDuration = _startResizingDuractionInTicks + maxAllowedChange;
                        double clampedStartTime = _startResizingTimeInTicks - maxAllowedChange;

                        double roundedClampedDuration = RoundTicksToGrid(clampedDuration);
                        double roundedClampedStartTime = RoundTicksToGrid(clampedStartTime);

                        _selectObjectController.MultipleResizingLeft(this, maxAllowedChange);
                        Rezise?.Invoke(roundedClampedStartTime - StartTimeInTicks);
                        StartTimeInTicks = roundedClampedStartTime;
                        ChangeDurationInTicks(roundedClampedDuration);
                        return;
                    }

                    _selectObjectController.MultipleResizingLeft(this, deltaTicks);
                    double newStartTimeInTicks = _startResizingTimeInTicks - deltaTicks;
                    double newDurationInTicks = _startResizingDuractionInTicks + deltaTicks;

                    newStartTimeInTicks = RoundTicksToGrid(newStartTimeInTicks);
                    newDurationInTicks = RoundTicksToGrid(newDurationInTicks);

                    Rezise?.Invoke(newStartTimeInTicks - StartTimeInTicks);
                    StartTimeInTicks = newStartTimeInTicks;
                    ChangeDurationInTicks(newDurationInTicks);
                }
            }
            else if (_wasResizing)
            {
                _wasResizing = false;
            }
        }

        public void ChangeDurationInTicks(double durationInTicks)
        {
            TimeDuractionInTicks = Mathf.Round((float)durationInTicks);
            UpdateVisuals();
        }

        private void OnScroll(ref ScrollTimeLineEvent scrollTimeLineEvent)
        {
            UpdateVisuals();
        }

        public void OnScrollPan(ref PanEvent panEvent)
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            float durationInBeats = (float)(TimeDuractionInTicks / TimeLineConverter.TICKS_PER_BEAT);
            rect.sizeDelta = new Vector2(
                durationInBeats * (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Zoom),
                rect.sizeDelta.y);

            CalculatePosition();
        }

        internal void CalculatePosition()
        {
            float startTimeInSeconds = (float)TimeLineConverter.Instance.TicksToSeconds(StartTimeInTicks);
            rect.anchoredPosition = new Vector2(
                _timeLineConverter.GetAnchorPositionFromTime(startTimeInSeconds) + rect.sizeDelta.x / 2,
                rect.anchoredPosition.y);
        }

        public void Select()
        {
            _trackObjectStorage.SelectObjectTrackObject(this);
        }

        public void SelectColor()
        {
            image.color = Color.yellow;
        }

        public void Deselect()
        {
            image.color = Color.gray;
        }

        public void OnMouseDown()
        {
            _startTrackObjectTicks = StartTimeInTicks;
            Vector2 mousePos = GetMousePosition();
            _startMouseXLocal = mousePos.x;
            _isDragging = true;
            _deathZonePass = false;
            _selectObjectController.StartMultipleMove(this);
        }

        public void SavePosition()
        {
            _startTrackObjectTicks = StartTimeInTicks;
        }

        public void OnMouseUp()
        {
            _isDragging = false;
        }

        private void Drag()
        {
            if (!_isDragging) return;

            if (transform.parent != TrackLine.RectTransform) transform.parent = TrackLine.RectTransform;
            rect.offsetMax = new Vector2(rect.offsetMax.x, 0);
            rect.offsetMin = new Vector2(rect.offsetMin.x, 0);

            UpdatePosition();
            CalculatePosition();
        }

        internal void UpdatePosition()
        {
            Vector2 currentMousePos = GetMousePosition();
            float currentMouseXLocal = currentMousePos.x;
            float mouseDeltaXLocal = currentMouseXLocal - _startMouseXLocal;
            double deltaTicks = AnchorPositionDeltaToTicks(mouseDeltaXLocal);
            if (Mathf.Abs(mouseDeltaXLocal) < deathZone && _deathZonePass == false)
            {
                return;
            }
            else
            {
                _deathZonePass = true;
            }

            int oldIndex = _trackStorage.GetIndex(TrackLine);
            TrackLine = _trackStorage.CheckTracks(this);
            int newIndex = _trackStorage.GetIndex(TrackLine);
            if (oldIndex != -1 && newIndex != -1 && Mathf.Abs(oldIndex - newIndex) > 0)
                _selectObjectController.MultipleChangeTrackLine(this, oldIndex - newIndex);

            _selectObjectController.MultipleMove(this, deltaTicks);
            StartTimeInTicks = RoundTicksToGrid(_startTrackObjectTicks + deltaTicks);
            _trackObjectStorage.UpdatePositionSelectedTrackObject();
        }

        internal void AddTicksMove(double deltaTicks)
        {
            StartTimeInTicks = RoundTicksToGrid(_startTrackObjectTicks + deltaTicks);
            _trackObjectStorage.UpdatePositionSelectedTrackObject();
            CalculatePosition();
        }

        internal void AddLineTrackIndex(int addedIndex)
        {
            TrackLine = _trackStorage.GetTrackLine( _trackStorage.GetIndex(TrackLine) - addedIndex);
            transform.parent = TrackLine.RectTransform;
            RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x, 0);
        }

        private double AnchorPositionDeltaToTicks(float deltaAnchorPosition)
        {
            float pixelsPerBeat = _timeLineScroll.Zoom;
            float beatsDelta = deltaAnchorPosition / pixelsPerBeat;
            return beatsDelta * TimeLineConverter.TICKS_PER_BEAT;
        }

        #region Конвертация методов
        

        private double RoundTicksToGrid(double ticks)
        {
            float timeInSeconds = (float)TimeLineConverter.Instance.TicksToSeconds(ticks);
            float roundedTimeInSeconds = _gridUI.RoundTimeToGrid(timeInSeconds);
            return TimeLineConverter.Instance.SecondsToTicks(roundedTimeInSeconds);
        }

        #endregion
    }
}