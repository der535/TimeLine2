using EventBus;
using TimeLine;
using TimeLine.EventBus.Events.TimeLine;
using UnityEngine;
using Zenject;

public class CurrentTimeMarkerRenderer : MonoBehaviour
{
    [SerializeField] private GameObject marker;
    public float TimeLineAnchoredPosition => marker.GetComponent<RectTransform>().anchoredPosition.x;

    private Scroll _scroll;
    private Main _main;
    private GameEventBus _gameEventBus;
    private TimeLineSettings _timeLineSettings;
    private TimeLineScroll _timeLineScroll;

    private double _ticksSaved;

    [Inject]
    private void Construct(Main main, Scroll scroll, GameEventBus gameEventBus, TimeLineScroll timeLineScroll, TimeLineSettings timeLineSettings)
    {
        _scroll = scroll;
        _main = main;
        _gameEventBus = gameEventBus;
        _timeLineSettings = timeLineSettings;
        _timeLineScroll = timeLineScroll;
    }

    private void Awake()
    {
        _gameEventBus.SubscribeTo<TickSmoothTimeEvent>(OnTimeChangedSmooth);
        _gameEventBus.SubscribeTo<PanEvent>(OnScrollPan);
    }

    public void OnTimeChangedSmooth(ref TickSmoothTimeEvent timeEvent)
    {
        // Конвертируем тики в позицию на таймлайне
        double beats = timeEvent.Time / Main.TICKS_PER_BEAT;
        double seconds = beats * (60.0 / _main.MusicData.bpm);
        
        float positionX = (float)(seconds * (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan) * (_main.MusicData.bpm / 60.0));
        
        marker.transform.localPosition = new Vector3(
            positionX,
            marker.transform.localPosition.y,
            marker.transform.localPosition.z
        );

        _ticksSaved = timeEvent.Time;
    }

    public void OnScrollPan(ref PanEvent panEvent)
    {
        // Используем сохраненные тики и BPM для пересчета позиции
        double beats = _ticksSaved / Main.TICKS_PER_BEAT;
        double seconds = beats * (60.0 / _main.MusicData.bpm);
        
        float positionX = (float)(seconds * (_timeLineSettings.DistanceBetweenBeatLines + panEvent.PanOffset) * (_main.MusicData.bpm / 60.0));
        
        marker.transform.localPosition = new Vector3(
            positionX,
            marker.transform.localPosition.y,
            marker.transform.localPosition.z
        );
    }
}