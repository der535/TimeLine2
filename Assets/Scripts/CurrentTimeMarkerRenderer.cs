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

    private float _timeSaved;

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
        _gameEventBus.SubscribeTo<SmoothTimeEvent>(OnTimeChangedSmooth);
        _gameEventBus.SubscribeTo<PanEvent>(OnScrollPan);
    }

    public void OnTimeChangedSmooth(ref SmoothTimeEvent timeEvent)
    {
        marker.transform.localPosition =
            new Vector3(
                timeEvent.Time * (_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Pan) *
                (_main.MusicDataSo.bpm / 60),
                marker.transform.localPosition.y,
                marker.transform.localPosition.z);
        _timeSaved = timeEvent.Time;
    }

    public void OnScrollPan(ref PanEvent panEvent)
    {
        marker.transform.localPosition =
            new Vector3(
                _timeSaved * (_timeLineSettings.DistanceBetweenBeatLines + panEvent.PanOffset) *
                (_main.MusicDataSo.bpm / 60),
                marker.transform.localPosition.y,
                marker.transform.localPosition.z);
    }
}