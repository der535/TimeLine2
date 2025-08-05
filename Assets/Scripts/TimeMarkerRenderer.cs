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
    [Space] private Dictionary<RectTransform, Vector2> _lines = new();
    
    private TimeLineSettings _timeLineSettings;
    private GameEventBus _gameEventBus;
    private MainObjects _mainObjects;

    [Inject]
    private void Construct(TimeLineSettings timeLineSettings, GameEventBus gameEventBus, MainObjects mainObjects)
    {
        _timeLineSettings = timeLineSettings;
        _gameEventBus = gameEventBus;
        _mainObjects = mainObjects;
    }
    private void Awake()
    {
        _gameEventBus.SubscribeTo<PanEvent>(SetPan);
        
        for (int i = 0; i < countBeatLines; i++)
        {
            Vector3 position = new Vector3(_timeLineSettings.DistanceBetweenBeatLines * i, 0, 0);
            TimeMarker beatLine = Instantiate(beatLinesPrefab, _mainObjects.ContentRectTransform).GetComponent<TimeMarker>();
            beatLine.Set(i);
            RectTransform beatLineRectTransform = beatLine.GetComponent<RectTransform>();
            beatLineRectTransform.anchoredPosition = position;
            _lines.Add(beatLineRectTransform, beatLineRectTransform.anchoredPosition);
        }
    }

    public void SetPan(ref PanEvent panEvent)
    {
        float scale = 1 + panEvent.PanOffset / _timeLineSettings.DistanceBetweenBeatLines;
        foreach (KeyValuePair<RectTransform, Vector2> entry in _lines)
        {
            entry.Key.anchoredPosition =
                new Vector2(entry.Value.x * scale, 0);
        }
    }
}