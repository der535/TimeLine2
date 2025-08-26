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

        private bool _isDragging;
        private bool _isResizing;
        private bool _isRightResizing;

        private float _mouseOffset;
        private float _startResizingDuraction;
        private float _startResizingTime;

        private Vector2 _startMousePosition;

        #endregion

        internal float StartTime { get; private set; }
        internal float TimeDuraction { get; private set; }
        internal TrackLine TrackLine { get; private set; }
        internal float BeatDuraction { get; private set; } //В секундах
        internal string Name { get; private set; }


        [Inject]
        private void Construct(
            TimeLineSettings timeLineSettings,
            TrackObjectStorage trackObjectStorage,
            TrackStorage trackStorage,
            TimeLineConverter timeLineConverter,
            MainObjects mainObjects,
            GameEventBus gameEventBus,
            TimeLineScroll timeLineScroll,
            GridUI gridUI)
        {
            _gridUI = gridUI;
            _timeLineSettings = timeLineSettings;
            _trackObjectStorage = trackObjectStorage;
            _trackStorage = trackStorage;
            _timeLineConverter = timeLineConverter;
            _mainObjects = mainObjects;
            _gameEventBus = gameEventBus;
            _timeLineScroll = timeLineScroll;
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

        internal void Setup(TrackObjectSO trackObjectSo, TrackLine trackLine, float startTime)
        {
            StartTime = startTime;
            TrackLine = trackLine;

            BeatDuraction = trackObjectSo.startLiveTime;
            TimeDuraction = _timeLineConverter.GetTimeFromBeatPosition(BeatDuraction);

            rect.sizeDelta = new Vector2(
                BeatDuraction * (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan),
                rect.sizeDelta.y);

            Name = trackObjectSo.name;
            nameText.text = trackObjectSo.name;

            rect.anchoredPosition =
                new Vector2(
                    _timeLineConverter.GetAnchorPositionFromTime(StartTime) + rect.sizeDelta.x / 2,
                    rect.anchoredPosition.y);
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
            _startResizingDuraction = TimeDuraction;
            _isRightResizing = true;
            _isResizing = isResizing;
            if (_isResizing)
                _startMousePosition = GetMousePosition();
        }

        public void SetResizeLeft(bool isResizing)
        {
            _startResizingDuraction = TimeDuraction;
            _startResizingTime = StartTime;
            _isRightResizing = false;
            _isResizing = isResizing;
            if (_isResizing)
                _startMousePosition = GetMousePosition();
            else
            {

            }
        }

        private void Update()
        {
            Drag();
            if (_isResizing)
            {
                if (_isRightResizing)
                {
                    ChangeDuration(_startResizingDuraction + (_timeLineConverter.GetTimeFromAnchorPosition(
                        GetMousePosition().x - _startMousePosition.x
                    )));
                }
                else
                {
                    float newTime = _timeLineConverter.GetTimeFromAnchorPosition(
                        _startMousePosition.x - GetMousePosition().x);
                    
                    float difference = (newTime + _startResizingDuraction) - _startResizingDuraction;
                    
                    print(difference);
                    print(_startResizingTime);
                    print(_startResizingTime - difference);
                    
                    StartTime = _gridUI.RoundTimeToGrid(_startResizingTime - difference);
                    
                    ChangeDuration(_startResizingDuraction + newTime,
                        -((newTime + _startResizingDuraction) - _startResizingDuraction));
                }
            }
        }

        public void ChangeDuration(float duration, float timeOffset = 0)
        {
            TimeDuraction = _gridUI.RoundTimeToGrid(duration);
            BeatDuraction = _timeLineConverter.GetBeatPositionFromTime(TimeDuraction);
            
            
            

            
            
            

            rect.sizeDelta = new Vector2(
                BeatDuraction * (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan),
                rect.sizeDelta.y);
            CalculatePosition();
        }

        private void OnScroll(ref ScrollTimeLineEvent scrollTimeLineEvent)
        {
            CalculatePosition();
        }

        public void OnScrollPan(ref PanEvent panEvent)
        {
            rect.sizeDelta = new Vector2(
                BeatDuraction * (_timeLineSettings.DistanceBetweenBeatLines + panEvent.PanOffset),
                rect.sizeDelta.y);
            CalculatePosition();
        }

        private void CalculatePosition()
        {
            rect.anchoredPosition =
                new Vector2(
                    _timeLineConverter.GetAnchorPositionFromTime(StartTime) + rect.sizeDelta.x / 2,
                    rect.anchoredPosition.y);
        }

        private void UpdatePosition()
        {
            StartTime = _gridUI.RoundTimeToGrid(_timeLineConverter.GetTimeFromBeatPosition(
                _timeLineConverter.GetCursorBeatPosition(_timeLineScroll.Pan, (rect.sizeDelta.x / 2) + _mouseOffset)));
            _trackObjectStorage.UpdatePositionSelectedTrackObject();
        }

        public void Select()
        {
            _trackObjectStorage.SelectObject(this);
            image.color = Color.yellow;
        }

        public void Deselect()
        {
            image.color = Color.gray;
        }

        public void OnMouseDown()
        {
            float mousePosition =
                Mouse.current.position.ReadValue().x - _mainObjects.CanvasRectTransform.sizeDelta.x / 2;
            
            _mouseOffset =  mousePosition - rect.anchoredPosition.x;

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
    }
}