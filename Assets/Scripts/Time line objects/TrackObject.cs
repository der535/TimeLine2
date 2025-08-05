using System;
using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.Installers;
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

        private TimeLineSettings _timeLineSettings;
        private TrackObjectStorage _trackObjectStorage;
        private TrackStorage _trackStorage;
        private TimeLineConverter _timeLineConverter;
        private Scroll _scroll;
        private MainObjects _mainObjects;
        private Main _main;
        private GameEventBus _gameEventBus;
        private TimeLineScroll _timeLineScroll;


        internal float StartTime { get; private set; }
        internal float TimeDuraction { get; private set; }
        internal TrackLine TrackLine { get; private set; }
        internal float BeatDuraction { get; private set; } //В секундах
        internal string Name { get; private set; }

        public RectTransform RectTransform => rect;
        
        private bool _isDragging;
        private float _mouseOffset;

        [Inject]
        private void Construct(
            TimeLineSettings timeLineSettings,
            TrackObjectStorage trackObjectStorage,
            TrackStorage trackStorage,
            TimeLineConverter timeLineConverter,
            Scroll scroll,
            MainObjects mainObjects,
            GameEventBus gameEventBus,
            Main main,
            TimeLineScroll timeLineScroll)
        {
            _timeLineSettings = timeLineSettings;
            _trackObjectStorage = trackObjectStorage;
            _trackStorage = trackStorage;
            _timeLineConverter = timeLineConverter;
            _scroll = scroll;
            _mainObjects = mainObjects;
            _gameEventBus = gameEventBus;
            _main = main;
            _timeLineScroll = timeLineScroll;
        }

        internal void Awake()
        {
            _gameEventBus.SubscribeTo<ScrollTimeLineEvent>(OnScroll);
            _gameEventBus.SubscribeTo<PanEvent>(OnScrollPan);
        }

        internal void Setup(TrackObjectSO trackObjectSo, TrackLine trackLine, float startTime)
        {
            StartTime = startTime;
            TrackLine = trackLine;

            BeatDuraction = trackObjectSo.startLiveTime;
            TimeDuraction =  _timeLineConverter.GetTimeFromBeatPosition(BeatDuraction);


            rect.sizeDelta = new Vector2(BeatDuraction * (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan),
                rect.sizeDelta.y);

            Name = trackObjectSo.name;
            nameText.text = trackObjectSo.name;

            rect.anchoredPosition =
                new Vector2(
                    _timeLineConverter.GetAnchorPositionFromTime(StartTime) + rect.sizeDelta.x / 2,
                    rect.anchoredPosition.y);
        }

        private void Update()
        {
            Drag();
        }

        public void ChangeDuration(float duration)
        {
            TimeDuraction = duration;
            BeatDuraction = _timeLineConverter.GetBeatPositionFromTime(TimeDuraction);
            
            rect.sizeDelta = new Vector2(BeatDuraction * (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan),
                rect.sizeDelta.y);
            rect.anchoredPosition =
                new Vector2(
                    _timeLineConverter.GetAnchorPositionFromTime(StartTime) + rect.sizeDelta.x / 2,
                    rect.anchoredPosition.y);
        }

        private void OnScroll(ref ScrollTimeLineEvent scrollTimeLineEvent)
        {
            rect.anchoredPosition =
                new Vector2(
                    _timeLineConverter.GetAnchorPositionFromTime(StartTime) + rect.sizeDelta.x / 2,
                    rect.anchoredPosition.y);
        }

        public void OnScrollPan(ref PanEvent panEvent)
        {
            rect.sizeDelta = new Vector2(BeatDuraction * (_timeLineSettings.DistanceBetweenBeatLines + panEvent.PanOffset),
                rect.sizeDelta.y);
            rect.anchoredPosition =
                new Vector2(
                    _timeLineConverter.GetAnchorPositionFromTime(StartTime) + rect.sizeDelta.x / 2,
                    rect.anchoredPosition.y);
        }

        private void UpdatePosition()
        {
            StartTime = _timeLineConverter.GetTimeFromBeatPosition(
                _timeLineConverter.GetCursorBeatPosition(_timeLineScroll.Pan, (rect.sizeDelta.x / 2)+_mouseOffset));
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

            _mouseOffset = mousePosition - rect.anchoredPosition.x;
            
            _isDragging = true;
        }
        
        public void OnMouseUp()
        {
            _isDragging = false;
        }

        private void Drag()
        {
            if(_isDragging == false) return;
            
            TrackLine = _trackStorage.CheckTracks(this);
            if(transform.parent != TrackLine.RectTransform) transform.parent = TrackLine.RectTransform;
            rect.offsetMax = new Vector2(rect.offsetMax.x, 0);
            rect.offsetMin = new Vector2(rect.offsetMin.x, 0);

            float mousePosition =
                Mouse.current.position.ReadValue().x - _mainObjects.CanvasRectTransform.sizeDelta.x / 2;
            

            rect.anchoredPosition =
                new Vector2(mousePosition - _mouseOffset,
                    rect.anchoredPosition.y);
            UpdatePosition();
        }
    }
}