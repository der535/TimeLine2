using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe.KeyframeTimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class KeyframeTimeLine : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        [SerializeField] private TimeLineKeyframeScroll timeLineKeyframeScroll;

        private TrackObject _trackObject;

        private Main _main;
        private GameEventBus _gameEventBus;
        private TimeLineSettings _timeLineSettings;
        private GridUI _gridUI;
        private TrackObject _trackObjectData;

        [Inject]
        private void Construct(Main main, GameEventBus gameEventBus, TimeLineSettings timeLineSettings,
            GridUI gridUI)
        {
            _gridUI = gridUI;
            _main = main;
            _gameEventBus = gameEventBus;
            _timeLineSettings = timeLineSettings;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo<TickSmoothTimeEvent>(OnTimeChangedSmoothTicks);
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) => OnSelectTrackObject(data.Tracks[^1]));
            _gameEventBus.SubscribeTo<DragTrackObjectEvent>(OnDragTrackObject);
            _gameEventBus.SubscribeTo(
                (ref EventBus.Events.KeyframeTimeLine.PanEvent data) =>
                {
                    if(_trackObjectData != null)
                        UpdatePosition(_main.TicksCurrentTime(), _trackObjectData);
                });
        }

        public void OnTimeChangedSmoothTicks(ref TickSmoothTimeEvent tickEvent)
        {
            UpdatePosition(tickEvent.Time);
        }

        public void OnSelectTrackObject(TrackObjectData trackObjectData)
        {
            _trackObject = trackObjectData.trackObject;
            UpdatePosition(_main.TicksCurrentTime());
        }

        public void OnDragTrackObject(ref DragTrackObjectEvent dragTrackObjectEvent)
        {
            UpdatePosition(_main.TicksCurrentTime(), dragTrackObjectEvent.Track.trackObject);
        }

        private void UpdatePosition(double ticks, TrackObject trackObject)
        {
            _trackObjectData = trackObject;
            // Конвертируем StartTime трек-объекта в тики
            double startTimeTicks = trackObject.StartTimeInTicks;

            // Вычисляем разницу во времени в тиках
            double timeDiffTicks = ticks - startTimeTicks;

            // Конвертируем разницу в тиках в секунды для позиционирования
            double timeDiffSeconds = TicksToSeconds(timeDiffTicks, _main.MusicData.bpm);

            // Вычисляем позицию
            float positionX = (float)(timeDiffSeconds *
                                      (_timeLineSettings.DistanceBetweenBeatLines + timeLineKeyframeScroll.Pan) *
                                      (_main.MusicData.bpm / 60));

            rect.anchoredPosition = new Vector2(positionX, rect.anchoredPosition.y);
        }

        private void UpdatePosition(double ticks)
        {
            if (_trackObject)
                UpdatePosition(ticks, _trackObject);
        }

        private double SecondsToTicks(double seconds, double bpm)
        {
            return seconds * (bpm * Main.TICKS_PER_BEAT / 60.0);
        }

        private double TicksToSeconds(double ticks, double bpm)
        {
            return ticks * (60.0 / (bpm * Main.TICKS_PER_BEAT));
        }

        private void OnDestroy()
        {
            _gameEventBus.UnsubscribeFrom<TickSmoothTimeEvent>(OnTimeChangedSmoothTicks);
            _gameEventBus.UnsubscribeFrom<SelectObjectEvent>((ref SelectObjectEvent data) =>
                OnSelectTrackObject(data.Tracks[^1]));
            _gameEventBus.UnsubscribeFrom<DragTrackObjectEvent>(OnDragTrackObject);
        }
    }
}