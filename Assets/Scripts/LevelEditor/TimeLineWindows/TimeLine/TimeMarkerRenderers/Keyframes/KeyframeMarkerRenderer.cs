using System.Collections.Generic;
using System.Globalization;
using EventBus;
using NaughtyAttributes;
using TimeLine;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.Installers;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;
using PanEvent = TimeLine.EventBus.Events.KeyframeTimeLine.PanEvent;

public class KeyframeMarkerRenderer : MonoBehaviour
{
    // [SerializeField] private RectTransform timeLineRectTransform;
    // [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform timeMarkersRoot;
    [SerializeField] private RectTransform rootObject;
    [SerializeField] private Canvas canvas;
    [Space]
    [SerializeField] private TimeMarker beatLinesPrefab;
    [SerializeField] private int countBeatLines;
    [SerializeField] private float minDistance = 50;
    [SerializeField] private Color major = Color.white;
    [SerializeField] private Color between = Color.white;

    private List<TimeMarker> _lines = new();
    private TimeLineSettings _timeLineSettings;
    private GameEventBus _gameEventBus;
    private TimeLineConverter _timeLineConverter;
    private TimeLineKeyframeScroll _keyframeScroll;

    private float skipLines;

    [Inject]
    private void Construct(TimeLineSettings timeLineSettings, GameEventBus gameEventBus,
        TimeLineKeyframeScroll keyframeScroll, TimeLineConverter timeLineConverter)
    {
        _timeLineSettings = timeLineSettings;
        _gameEventBus = gameEventBus;
        _keyframeScroll = keyframeScroll;
        _timeLineConverter = timeLineConverter;
    }

    private void Awake()
    {
        _gameEventBus.SubscribeTo<ScrollTimeLineKeyframeEvent>((ref ScrollTimeLineKeyframeEvent f) => CalculateDistance(),-1);
        _gameEventBus.SubscribeTo<PanEvent>((ref PanEvent data) => CalculateDistance(), -1);
        _gameEventBus.SubscribeTo<ZoomBezier>((ref ZoomBezier data) => CalculateDistance());

        for (int i = 0; i < countBeatLines; i++)
        {
            _lines.Add(Instantiate(beatLinesPrefab, timeMarkersRoot));
        }
    }

    [Button]
    private void CalculateDistance()
    {
        // Начинаем с 0, затем перейдем к 3, 6, 12...
        skipLines = 0;
        float distance = _timeLineConverter.TicksToPositionX(skipLines + 1, _keyframeScroll.Zoom);

        // Если 0 не подходит под условие дистанции, начинаем цикл со значения 3
        if (distance < minDistance)
        {
            skipLines = 3;
            while (skipLines <= int.MaxValue) // Ваш предохранитель
            {
                distance = _timeLineConverter.TicksToPositionX(skipLines + 1, _keyframeScroll.Zoom);

                if (distance >= minDistance)
                    break;

                skipLines *= 2; // Удваиваем: 3 -> 6 -> 12 -> 24...
            }
        }

        PoseLines();
    }

    // private float GetMinPosition()
    // {
    //     var a = -(timeLineRectTransform.rect.width / 2);
    //     var b = _mainObjects.ContentRectTransform.offsetMin.x;
    //     var e = _timeLineConverter.PositionXToTicks(a - b, _keyframeScroll.Zoom);
    //     double remainder = e % skipLines;
    //     double result = e - remainder; 
    //     return (float)result;
    // }

    [Button]
    private float GetMinPosition2()
    {
        var a = -(timeMarkersRoot.rect.width / 2);
        var b = rootObject.offsetMin.x;
        var e = _timeLineConverter.PositionXToTicks(a - b, _keyframeScroll.Zoom);

        // ФИКС: Если skipLines == 0, остаток всегда 0 (начинаем с ближайшего тика)
        if (skipLines <= 0) return Mathf.Floor((float)e);

        double remainder = e % skipLines;
        double result = e - remainder; 
        return (float)result;
    }

    public void PoseLines()
    {
        float zoom = _keyframeScroll.Zoom;
        float minPosition = GetMinPosition2();

        // ФИКС: Если skipLines 0, шаг между палками 1 тик, иначе вычисляем суб-шаг
        float subStep = (skipLines <= 0) ? 1f : Mathf.Max(skipLines / 4f, 3f); 

        for (var index = 0; index < _lines.Count; index++)
        {
            var line = _lines[index];
            float currentTick = minPosition + (index * subStep);

            // ФИКС: Если skipLines == 0, то каждая линия - Major
            bool isMajor;
            if (skipLines <= 0)
            {
                isMajor = true;
            }
            else
            {
                isMajor = Mathf.Abs(currentTick % skipLines) < 0.001f || 
                          Mathf.Abs((currentTick % skipLines) - skipLines) < 0.001f;
            }

            if (isMajor)
            {
                // Рисуем число (currentTick) на каждой палке
                line.Setup(canvas, currentTick.ToString(CultureInfo.InvariantCulture), major);
            }
            else
            {
                line.Setup(canvas, string.Empty, between);
            }

            line.RectTransform.anchoredPosition = new Vector2(
                _timeLineConverter.TicksToPositionX(currentTick, zoom),
                line.RectTransform.anchoredPosition.y
            );
        }
    }
}