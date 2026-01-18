using System.Collections.Generic;
using System.Globalization;
using EventBus;
using NaughtyAttributes;
using TimeLine;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

public class VerticalGraphAnimationMarkerRenderer : MonoBehaviour
{
    [SerializeField] private RectTransform pointsRoot;
    [SerializeField] private RectTransform rootObject;
    [SerializeField] private Canvas canvas;
    [Space] [SerializeField] private TimeMarker beatLinesPrefab;
    [SerializeField] private int countBeatLines;
    [SerializeField] private float minDistance = 50;

    private List<TimeMarker> _lines = new();
    private GameEventBus _gameEventBus;
    private VerticalBezierZoom _verticalBezierZoom;
    private VerticalBezierScroll _verticalBezierScroll;
    private ThemeStorage _themeStorage;

    private float _skipLines;

    [Inject]
    private void Construct(TimeLineSettings timeLineSettings, GameEventBus gameEventBus,
        VerticalBezierZoom verticalBezierZoom, TimeLineConverter timeLineConverter,
        VerticalBezierScroll verticalBezierScroll, ThemeStorage themeStorage)
    {
        _gameEventBus = gameEventBus;
        _verticalBezierZoom = verticalBezierZoom;
        _verticalBezierScroll = verticalBezierScroll;
        _themeStorage = themeStorage;
    }

    private void Awake()
    {
        _gameEventBus.SubscribeTo((ref ScrollBezier data) => CalculateDistance(), -1);
        _gameEventBus.SubscribeTo((ref ZoomBezier data) => CalculateDistance(),-1);
        _gameEventBus.SubscribeTo((ref ThemeChangedEvent data) =>
        {
            PoseLines();
        });

        for (int i = 0; i < countBeatLines; i++)
        {
            _lines.Add(Instantiate(beatLinesPrefab, rootObject));
        }
    }


    private float GetAnchorPositionFromValue(float value, float pan)
    {
        float scrollFactor = pan;
        float position = value * scrollFactor; // ← Скролл НЕ добавляем!


        return position;
    }

    private float GetValueFromAnchorPosition(float position, float pan)
    {
        float scrollFactor = pan;
        float value = position / scrollFactor; // ← Скролл НЕ добавляем!

        return value;
    }

    private readonly float[] _stepValues = { 
        0.01f, 0.02f, 0.05f, 0.1f, 0.2f, 0.5f, 1f, 2f, 5f, 10f, 20f, 50f, 100f, 200f, 500f, 1000f, 2000f, 5000f, 10000f 
    };

    [Button]
    private void CalculateDistance()
    {
        float currentZoom = _verticalBezierZoom.Zoom;
        _skipLines = _stepValues[0]; // Начинаем с минимального 0.01

        foreach (float step in _stepValues)
        {
            _skipLines = step;
            // Дистанция между двумя соседними делениями при текущем шаге
            float distance = _skipLines * currentZoom;

            if (distance >= minDistance)
                break;
        }

        PoseLines();
    }

    [Button]
    private float GetMinPosition2()
    {
        // Высота и смещение
        float viewPortHalfHeight = pointsRoot.rect.height / 2f;
        float scrollOffset = pointsRoot.offsetMax.y;

        // Получаем значение в "тиках/единицах" для нижней границы экрана
        float valueAtBottom =
            GetValueFromAnchorPosition(-(viewPortHalfHeight + scrollOffset), _verticalBezierZoom.Zoom);

        // Округляем до ближайшего шага вниз
        float remainder = valueAtBottom % _skipLines;
        float result = valueAtBottom - remainder;

        // Если мы ушли в отрицательные значения, % может вести себя иначе, 
        // корректируем, чтобы всегда возвращать точку СТАРТА отрисовки ниже экрана
        if (valueAtBottom < 0) result -= _skipLines;

        return result;
    }

    public void PoseLines()
    {
        float zoom = _verticalBezierZoom.Zoom;
        float scroll = _verticalBezierScroll.VerticalScroll;
        float minPosition = GetMinPosition2();

        // subStep — это расстояние между палками в списке _lines.
        // Если мы хотим, чтобы между Major-линиями (числами) были промежуточные, 
        // оставим логику деления, но для дробных чисел лучше рисовать каждую линию как Major,
        // если шаг уже очень мелкий (0.01).
        float subStep = _skipLines;

        for (var index = 0; index < _lines.Count; index++)
        {
            var line = _lines[index];
            float currentValue = minPosition + (index * subStep);

            // Проверка на Major-линию с учетом точности float
            // Для дробных значений обычно выгодно подписывать каждую линию, 
            // либо каждую 5-ю, если они слишком плотные.
            bool isMajor = true;

            if (isMajor)
            {
                // Форматирование "0.##" уберет лишние нули (0.10 -> 0.1)
                line.Setup(canvas, currentValue.ToString("0.##", CultureInfo.InvariantCulture), _themeStorage.value.timeMarkerPrimary, _themeStorage.value.timeMarkerText);
            }
            else
            {
                line.Setup(canvas, string.Empty, _themeStorage.value.timeMarkerPrimary, _themeStorage.value.timeMarkerText);
            }

            float yPos = GetAnchorPositionFromValue(currentValue, zoom) + scroll;
            line.RectTransform.anchoredPosition = new Vector2(line.RectTransform.anchoredPosition.x, yPos);

            // Оптимизация: скрываем линию, если она вышла за пределы видимости (опционально)
            // line.gameObject.SetActive(Mathf.Abs(yPos) < pointsRoot.rect.height);
        }
    }
}