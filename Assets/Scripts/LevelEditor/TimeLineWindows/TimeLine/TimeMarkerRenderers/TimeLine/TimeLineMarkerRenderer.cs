using System.Collections.Generic;
using System.Globalization;
using EventBus;
using NaughtyAttributes;
using TimeLine;
using TimeLine.EventBus.Events.Input;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.Installers;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;
using PanEvent = TimeLine.PanEvent;

public class TimeLineMarkerRenderer : MonoBehaviour
{
    [Space] [SerializeField] private TimeMarker beatLinesPrefab;
    [Space] [SerializeField] private RectTransform timeLineRectTransform;
    [SerializeField] private int countBeatLines;
    [SerializeField] private Canvas canvas;
    [Space] private List<TimeMarker> _lines = new();
    [SerializeField] private float minDistance = 50;
private ThemeStorage _themeStorage;

    private TimeLineSettings _timeLineSettings;
    private GameEventBus _gameEventBus;
    private MainObjects _mainObjects;
    private TimeLineScroll _timeLineScroll;
    private TimeLineConverter _timeLineConverter;

    private float skipLines;

    [Inject]
    private void Construct(TimeLineSettings timeLineSettings, GameEventBus gameEventBus, MainObjects mainObjects,
        TimeLineScroll timeLineScroll, TimeLineConverter timeLineConverter, ThemeStorage themeStorage)
    {
        _timeLineSettings = timeLineSettings;
        _gameEventBus = gameEventBus;
        _mainObjects = mainObjects;
        _timeLineScroll = timeLineScroll;
        _timeLineConverter = timeLineConverter;
        _themeStorage = themeStorage;
    }

    private void Awake()
    {
        _gameEventBus.SubscribeTo<PanEvent>((ref PanEvent f) => CalculateDistance());
        _gameEventBus.SubscribeTo<ScrollTimeLineEvent>((ref ScrollTimeLineEvent data) => CalculateDistance());
        _gameEventBus.SubscribeTo((ref ThemeChangedEvent data) =>
        {
            PoseLines();
        });
        _gameEventBus.SubscribeTo((ref MusicLoadedEvent musicLoadedEvent) =>
        {
            CalculateDistance();
            PoseLines();
        });

        for (int i = 0; i < countBeatLines; i++)
        {
            _lines.Add(Instantiate(beatLinesPrefab, _mainObjects.ContentRectTransform));
        }
    }

    [Button]
    private void CalculateDistance()
    {
        // Начинаем с 0, затем перейдем к 3, 6, 12...
        skipLines = 0;
        float distance = _timeLineConverter.TicksToPositionX(skipLines + 1, _timeLineScroll.Zoom);

        // Если 0 не подходит под условие дистанции, начинаем цикл со значения 3
        if (distance < minDistance)
        {
            skipLines = 3;
            while (skipLines <= 1000) // Ваш предохранитель
            {
                distance = _timeLineConverter.TicksToPositionX(skipLines + 1, _timeLineScroll.Zoom);

                if (distance >= minDistance)
                    break;

                skipLines *= 2; // Удваиваем: 3 -> 6 -> 12 -> 24...
            }
        }

        // print($"Distance: {distance}");
        // print($"SkipLines: {skipLines}");
        
        PoseLines();
    }

    private float GetMinPosition()
    {
        var a = -(timeLineRectTransform.rect.width / 2);
        var b = _mainObjects.ContentRectTransform.offsetMin.x;
        var e = _timeLineConverter.PositionXToTicks(a - b, _timeLineScroll.Zoom);
        double remainder = e % skipLines;
        double result = e - remainder; 
        return (float)result;
    }

    public void PoseLines()
    {
        float zoom = _timeLineScroll.Zoom;
        float minPosition = GetMinPosition();
    
        // Определяем шаг между промежуточными линиями
        // Если skipLines = 6, subStep = 3 (1 линия между)
        // Если skipLines = 12, subStep = 3 (3 линии между: 3, 6, 9)
        // Ограничиваем, чтобы промежуточных было не более 3 (т.е. минимум 4 интервала)
        float subStep = Mathf.Max(skipLines / 4f, 3f); 

        for (var index = 0; index < _lines.Count; index++)
        {
            var line = _lines[index];
            // Текущее значение времени/тиков для этой линии
            float currentTick = minPosition + (index * subStep);

            // Линия считается "главной", если она кратна skipLines
            // Используем небольшую дельту (0.001f) для защиты от погрешностей float
            bool isMajor = Mathf.Abs(currentTick % skipLines) < 0.001f || 
                           Mathf.Abs((currentTick % skipLines) - skipLines) < 0.001f;

            if (isMajor)
            {
                // Главная линия с текстом
                line.Setup(canvas, currentTick.ToString(CultureInfo.InvariantCulture), _themeStorage.value.timeMarkerPrimary, _themeStorage.value.timeMarkerText);
            }
            else
            {
                // Промежуточная линия без текста
                line.Setup(canvas, string.Empty,  _themeStorage.value.timeMarkerSecond, _themeStorage.value.timeMarkerText);
            }

            line.RectTransform.anchoredPosition = new Vector2(
                _timeLineConverter.TicksToPositionX(currentTick, zoom),
                line.RectTransform.anchoredPosition.y
            );
        }
    }
}