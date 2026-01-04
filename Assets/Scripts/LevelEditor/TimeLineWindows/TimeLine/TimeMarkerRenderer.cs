using System.Collections.Generic;
using EventBus;
using TimeLine;
using TimeLine.Installers;
using UnityEngine;
using Zenject;

public class TimeMarkerRenderer : MonoBehaviour
{
    [Space] [SerializeField] private GameObject beatLinesPrefab;
    [SerializeField] private int countBeatLines;
    [SerializeField] private Canvas canvas;
    [Space] private Dictionary<RectTransform, int> _lines = new();
    
    private TimeLineSettings _timeLineSettings;
    private GameEventBus _gameEventBus;
    private MainObjects _mainObjects;
    private TimeLineScroll _timeLineScroll;

    [Inject]
    private void Construct(TimeLineSettings timeLineSettings, GameEventBus gameEventBus, MainObjects mainObjects, TimeLineScroll timeLineScroll)
    {
        _timeLineSettings = timeLineSettings;
        _gameEventBus = gameEventBus;
        _mainObjects = mainObjects;
        _timeLineScroll = timeLineScroll;
    }
    private void Awake()
    {
        _gameEventBus.SubscribeTo<PanEvent>(SetPan);
        
        for (int i = 0; i < countBeatLines; i++)
        {
            Vector3 position = new Vector3((_timeLineSettings.DistanceBetweenBeatLines + _timeLineScroll.Zoom) * i, 0, 0);
            TimeMarker beatLine = Instantiate(beatLinesPrefab, _mainObjects.ContentRectTransform).GetComponent<TimeMarker>();
            beatLine.Setup(canvas, i);
            RectTransform beatLineRectTransform = beatLine.GetComponent<RectTransform>();
            beatLineRectTransform.anchoredPosition = position;
            _lines.Add(beatLineRectTransform, i);
        }
    }

    public void SetPan(ref PanEvent panEvent)
    {
        float scale = _timeLineSettings.DistanceBetweenBeatLines + panEvent.PanOffset;
        foreach (KeyValuePair<RectTransform, int> entry in _lines)
        {
            entry.Key.anchoredPosition =
                new Vector2(entry.Value * scale, 0);
        }
    }
}