using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.Core.MusicData;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class KeyframeTimeLine : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;

        [FormerlySerializedAs("timeLineKeyframeScroll")] [SerializeField]
        private TimeLineKeyframeZoom timeLineKeyframeZoom;

        private GameEventBus _gameEventBus;
        private TimeLineSettings _timeLineSettings;
        private TrackObjectData _trackObjectData;
        private M_MusicData _musicData;

        [Inject]
        private void Construct(GameEventBus gameEventBus, TimeLineSettings timeLineSettings, M_MusicData musicData)
        {
            _gameEventBus = gameEventBus;
            _timeLineSettings = timeLineSettings;
            _musicData = musicData;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo<TickSmoothTimeEvent>(OnTimeChangedSmoothTicks);
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                if(data.UpdateVisual)
                    OnSelectTrackObject(data.Tracks[^1]);
            });
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => OnSelectTrackObject(data.SelectedObjects[^1]));
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) => { _trackObjectData = null;});
            _gameEventBus.SubscribeTo<DragTrackObjectEvent>(OnDragTrackObject);
            _gameEventBus.SubscribeTo(
                (ref EventBus.Events.KeyframeTimeLine.KeyframeZoomEvent data) =>
                {
                    
                    if (_trackObjectData != null)
                        UpdatePosition(TimeLineConverter.Instance.TicksCurrentTime(), _trackObjectData);
                });
        }

        public void OnTimeChangedSmoothTicks(ref TickSmoothTimeEvent tickEvent)
        {
            UpdatePosition(tickEvent.Time);
        }

        public void OnSelectTrackObject(TrackObjectPacket trackObjectPacket)
        {
            _trackObjectData = trackObjectPacket.components.Data;
            UpdatePosition(TimeLineConverter.Instance.TicksCurrentTime());
        }

        public void OnDragTrackObject(ref DragTrackObjectEvent dragTrackObjectEvent)
        {
            UpdatePosition(TimeLineConverter.Instance.TicksCurrentTime(), dragTrackObjectEvent.Track.components.Data);
        }

        private void UpdatePosition(double ticks, TrackObjectData trackObject)
        {
            _trackObjectData = trackObject;

            // Конвертируем StartTime трек-объекта в тики
            double startTimeTicks = trackObject.GetGlobalTicksPosition();

            // Вычисляем разницу во времени в тиках
            double timeDiffTicks = ticks - startTimeTicks;

            // Конвертируем разницу в тиках в секунды для позиционирования
            double timeDiffSeconds = TicksToSeconds(timeDiffTicks, _musicData.bpm);

            // Вычисляем позицию
            float positionX = (float)(timeDiffSeconds *
                                      timeLineKeyframeZoom.Zoom *
                                      (_musicData.bpm / 60));


            rect.anchoredPosition = new Vector2(positionX, rect.anchoredPosition.y);
        }

        private void UpdatePosition(double ticks)
        {
            if (_trackObjectData != null)
            {
                UpdatePosition(ticks, _trackObjectData);
            }
        }

        private double SecondsToTicks(double seconds, double bpm)
        {
            return seconds * (bpm * TimeLineConverter.TICKS_PER_BEAT / 60.0);
        }

        private double TicksToSeconds(double ticks, double bpm)
        {
            return ticks * (60.0 / (bpm * TimeLineConverter.TICKS_PER_BEAT));
        }

        private void OnDestroy()
        {
            _gameEventBus.UnsubscribeFrom<TickSmoothTimeEvent>(OnTimeChangedSmoothTicks);
            _gameEventBus.UnsubscribeFrom((ref SelectObjectEvent data) =>
                OnSelectTrackObject(data.Tracks[^1]));
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => OnSelectTrackObject(data.SelectedObjects[^1]));

            _gameEventBus.UnsubscribeFrom<DragTrackObjectEvent>(OnDragTrackObject);
        }
    }
}