using System;
using EventBus;
using TimeLine;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.LevelEditor.Core.MusicData;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

public class CurrentTimeMarkerRenderer : MonoBehaviour
{
    [SerializeField] private GameObject marker;
    public float TimeLineAnchoredPosition => marker.GetComponent<RectTransform>().anchoredPosition.x;

    private Main _main;
    private GameEventBus _gameEventBus;
    private TimeLineSettings _timeLineSettings;
    private TimeLineScroll _timeLineScroll;
    private TimeLineConverter _timeLineConverter;
    private M_MusicData _musicData;

    private double _ticksSaved;

    [Inject]
    private void Construct(Main main, GameEventBus gameEventBus, TimeLineScroll timeLineScroll,
        TimeLineSettings timeLineSettings, TimeLineConverter timeLineConverter, M_MusicData musicData)
    {
        _main = main;
        _gameEventBus = gameEventBus;
        _timeLineSettings = timeLineSettings;
        _timeLineScroll = timeLineScroll;
        _timeLineConverter = timeLineConverter;
        _musicData = musicData;
    }

    private void Awake()
    {
        _gameEventBus.SubscribeTo<TickSmoothTimeEvent>(OnTimeChangedSmooth);
        _gameEventBus.SubscribeTo<PanEvent>(OnScrollPan);
    }
    
    public void OnTimeChangedSmooth(ref TickSmoothTimeEvent timeEvent)
    {
        Build(timeEvent.Time);
    }

    private void Build(double ticks)
    {
        if (double.IsNaN(ticks)) ticks = 0;
        // Конвертируем тики в позицию на таймлайне
        double beats = ticks / TimeLineConverter.TICKS_PER_BEAT;
        
        
        double seconds = beats * (60.0 / _musicData.bpm);
        
        
        float positionX = (float)(seconds * _timeLineScroll.Zoom *
                                  (_musicData.bpm / 60.0));

        marker.transform.localPosition = new Vector3(
            positionX,
            marker.transform.localPosition.y,
            marker.transform.localPosition.z
        );

        _ticksSaved = ticks;
    }

    public double GetTime()
    {
        return _timeLineConverter.GetTimeFromAnchorPosition(marker.transform.localPosition.x, _timeLineScroll.Zoom);
    }

    public void OnScrollPan(ref PanEvent panEvent)
    {
        // Используем сохраненные тики и BPM для пересчета позиции
        double beats = _ticksSaved / TimeLineConverter.TICKS_PER_BEAT;
        double seconds = beats * (60.0 / _musicData.bpm);

        float positionX = (float)(seconds * (_timeLineSettings.DistanceBetweenBeatLines + panEvent.PanOffset) *
                                  (_musicData.bpm / 60.0));

        marker.transform.localPosition = new Vector3(
            positionX,
            marker.transform.localPosition.y,
            marker.transform.localPosition.z
        );
    }
}