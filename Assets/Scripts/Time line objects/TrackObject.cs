using System;
using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.Installers;
using TimeLine.TimeLine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class TrackObject : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private RectTransform rect;

        #region Вспомогательные переменные

        private TimeLineSettings _timeLineSettings;
        private TrackObjectStorage _trackObjectStorage;
        private TrackStorage _trackStorage;
        private TimeLineConverter _timeLineConverter;
        private MainObjects _mainObjects;
        private GameEventBus _gameEventBus;
        private TimeLineScroll _timeLineScroll;
        private GridUI _gridUI;
        private Main _main;

        private bool _isDragging;
        private bool _isResizing;
        private bool _isRightResizing;

        private double _startResizingDuractionInTicks;
        private double _startResizingTimeInTicks;

        private Vector2 _startMousePosition;
        private double _startTrackObjectTicks;
        private double _startMouseTicks;
        private float _startMouseXLocal; // Добавлено: начальная позиция мыши в локальных координатах

        #endregion

        internal double StartTimeInTicks { get; private set; }
        internal double TimeDuractionInTicks { get; private set; }
        internal TrackLine TrackLine { get; private set; }
        internal string Name { get; private set; }

        // Свойства для обратной совместимости (в секундах)
        internal float StartTime => (float)TicksToSeconds(StartTimeInTicks);
        internal float TimeDuraction => (float)TicksToSeconds(TimeDuractionInTicks);
        internal float BeatDuraction => (float)(TimeDuractionInTicks / Main.TICKS_PER_BEAT);

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
            Main main)
        {
            _gridUI = gridUI;
            _timeLineSettings = timeLineSettings;
            _trackObjectStorage = trackObjectStorage;
            _trackStorage = trackStorage;
            _timeLineConverter = timeLineConverter;
            _mainObjects = mainObjects;
            _gameEventBus = gameEventBus;
            _timeLineScroll = timeLineScroll;
            _main = main;
        }

        internal void Awake()
        {
            _gameEventBus.SubscribeTo<ScrollTimeLineEvent>(OnScroll);
            _gameEventBus.SubscribeTo<PanEvent>(OnScrollPan);
        }

        private void OnDestroy()
        {
            _gameEventBus.UnsubscribeFrom<ScrollTimeLineEvent>(OnScroll);
            _gameEventBus.UnsubscribeFrom<PanEvent>(OnScrollPan);
        }

        internal void Rename(string name)
        {
            nameText.text = name;
        }

        internal void Setup(TrackObjectSO trackObjectSo, TrackLine trackLine, double startTimeInTicks)
        {
            StartTimeInTicks = startTimeInTicks;
            TrackLine = trackLine;

            // Конвертируем длительность из битов в тики
            TimeDuractionInTicks = trackObjectSo.startLiveTime * Main.TICKS_PER_BEAT;

            Name = trackObjectSo.name;
            nameText.text = trackObjectSo.name;

            UpdateVisuals();
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
                _startMousePosition = GetMousePosition();
        }

        public void SetResizeLeft(bool isResizing)
        {
            _startResizingDuractionInTicks = TimeDuractionInTicks;
            _startResizingTimeInTicks = StartTimeInTicks;
            _isRightResizing = false;
            _isResizing = isResizing;
            if (_isResizing)
                _startMousePosition = GetMousePosition();
        }

        private void Update()
        {
            Drag();
            if (_isResizing)
            {
                if (_isRightResizing)
                {
                    double deltaTicks = AnchorPositionToTicks(GetMousePosition().x - _startMousePosition.x);
                    ChangeDurationInTicks(_startResizingDuractionInTicks + deltaTicks);
                }
                else
                {
                    double deltaTicks = AnchorPositionToTicks(_startMousePosition.x - GetMousePosition().x);
                    double newStartTimeInTicks = _startResizingTimeInTicks - deltaTicks;
                    double newDurationInTicks = _startResizingDuractionInTicks + deltaTicks;
                    
                    // Округляем до сетки
                    newStartTimeInTicks = RoundTicksToGrid(newStartTimeInTicks);
                    newDurationInTicks = RoundTicksToGrid(newDurationInTicks);
                    
                    StartTimeInTicks = newStartTimeInTicks;
                    ChangeDurationInTicks(newDurationInTicks);
                }
            }
        }

        public void ChangeDurationInTicks(double durationInTicks)
        {
            TimeDuractionInTicks = durationInTicks;
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
            // Конвертируем тики в секунды для UI
            float durationInBeats = (float)(TimeDuractionInTicks / Main.TICKS_PER_BEAT);
            rect.sizeDelta = new Vector2(
                durationInBeats * (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan),
                rect.sizeDelta.y);
            
            CalculatePosition();
        }

        private void CalculatePosition()
        {
            float startTimeInSeconds = (float)TicksToSeconds(StartTimeInTicks);
            rect.anchoredPosition = new Vector2(
                _timeLineConverter.GetAnchorPositionFromTime(startTimeInSeconds) + rect.sizeDelta.x / 2,
                rect.anchoredPosition.y);
        }

        public void Select()
        {
            _trackObjectStorage.SelectObject(this);
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
            // Сохраняем начальные позиции при начале перетаскивания
            _startTrackObjectTicks = StartTimeInTicks;
            
            // Получаем позицию мыши в локальных координатах канваса
            Vector2 mousePos = GetMousePosition();
            _startMouseXLocal = mousePos.x;
            
            // Конвертируем позицию мыши в тики
            _startMouseTicks = AnchorPositionToTicks(_startMouseXLocal);
            
            _isDragging = true;
        }

        public void OnMouseUp()
        {
            _isDragging = false;
        }

        private void Drag()
        {
            if (_isDragging == false) return;
            
            TrackLine = _trackStorage.CheckTracks(this);
            if (transform.parent != TrackLine.RectTransform) transform.parent = TrackLine.RectTransform;
            rect.offsetMax = new Vector2(rect.offsetMax.x, 0);
            rect.offsetMin = new Vector2(rect.offsetMin.x, 0);
            
            UpdatePosition();
            CalculatePosition();
        }

        private void UpdatePosition()
        {
            // Получаем текущую позицию мыши в локальных координатах
            Vector2 currentMousePos = GetMousePosition();
            float currentMouseXLocal = currentMousePos.x;
            
            // Вычисляем смещение мыши в локальных координатах
            float mouseDeltaXLocal = currentMouseXLocal - _startMouseXLocal;
            
            // Конвертируем смещение в тики
            double deltaTicks = AnchorPositionDeltaToTicks(mouseDeltaXLocal);
            
            // Вычисляем новую позицию
            StartTimeInTicks = RoundTicksToGrid(_startTrackObjectTicks + deltaTicks);
            
            _trackObjectStorage.UpdatePositionSelectedTrackObject();
        }

        private double AnchorPositionDeltaToTicks(float deltaAnchorPosition)
        {
            // Вычисляем изменение времени на основе смещения в координатах
            // Учитываем текущий масштаб (DistanceBetweenBeatLines + Pan)
            float pixelsPerBeat = _timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan;
            float beatsDelta = deltaAnchorPosition / pixelsPerBeat;
            return beatsDelta * Main.TICKS_PER_BEAT;
        }

        #region Конвертация методов

        private double TicksToSeconds(double ticks)
        {
            return ticks * (60.0 / (_main.MusicDataSo.bpm * Main.TICKS_PER_BEAT));
        }

        private double SecondsToTicks(double seconds)
        {
            return seconds * (_main.MusicDataSo.bpm * Main.TICKS_PER_BEAT / 60.0);
        }

        private double AnchorPositionToTicks(float anchorPosition)
        {
            float timeInSeconds = _timeLineConverter.GetTimeFromAnchorPosition(anchorPosition);
            return SecondsToTicks(timeInSeconds);
        }

        private double TicksToAnchorPosition(double ticks)
        {
            float timeInSeconds = (float)TicksToSeconds(ticks);
            return _timeLineConverter.GetAnchorPositionFromTime(timeInSeconds);
        }

        private double BeatPositionToTicks(double beatPosition)
        {
            return beatPosition * Main.TICKS_PER_BEAT;
        }

        private double RoundTicksToGrid(double ticks)
        {
            float timeInSeconds = (float)TicksToSeconds(ticks);
            float roundedTimeInSeconds = _gridUI.RoundTimeToGrid(timeInSeconds);
            return SecondsToTicks(roundedTimeInSeconds);
        }

        #endregion
    }
}