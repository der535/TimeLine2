using System;
using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.LevelEditor.Core;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class BarBeatCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        private GameEventBus _gameEventBus;

        private double _oldBeat;

        [Inject]
        private void Construct(
            GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        public void Awake()
        {
            _gameEventBus.SubscribeTo<TickExactTimeEvent>(OnTimeChangedUnSmooth);
        }

        public void OnTimeChangedUnSmooth(ref TickExactTimeEvent timeEvent)
        {
            double currentTimeInTicks = timeEvent.Time;
            
            // Вычисляем текущую долю
            double currentBeat = Math.Ceiling(currentTimeInTicks / TimeLineConverter.TICKS_PER_BEAT);
            
            if (currentBeat != _oldBeat)
            {
                _oldBeat = currentBeat;
                _gameEventBus.Raise(new BeatEvent((int)currentBeat));
                
                // Также обновляем текстовое отображение
                UpdateTextDisplay(currentTimeInTicks);
            }
            else
            {
                // Обновляем только отображение, если доля не изменилась
                UpdateTextDisplay(currentTimeInTicks);
            }
        }


        private void UpdateTextDisplay(double currentTimeInTicks)
        {
            // Константы для преобразования
            const double beatsPerBar = 4.0;   // обычно 4 доли в такте
            const int stepsPerBeat = 4;       // 4 шага на долю (1/16 ноты)
            const int stepsPerBar = (int)(beatsPerBar * stepsPerBeat); // 16 шагов в такте

            // Вычисляем компоненты времени
            double totalBeats = currentTimeInTicks / TimeLineConverter.TICKS_PER_BEAT;
            double bars = totalBeats / beatsPerBar;

            // Целая часть - такты
            int wholeBars = (int)bars;

            // Дробная часть - доли и тики
            double fractionalBar = bars - wholeBars;
            double beatsInCurrentBar = fractionalBar * beatsPerBar;

            int wholeBeats = (int)beatsInCurrentBar;
            double fractionalBeat = beatsInCurrentBar - wholeBeats;
            int ticks = (int)(fractionalBeat * TimeLineConverter.TICKS_PER_BEAT);

            // Формат 1: bar:beat:tick (такты:доли:тики)
            string format1 = $"{wholeBars + 1}:{wholeBeats + 1}:{ticks:00}";

            // Формат 2: bar:step:tick (такты:шаги:тики)
            // Вычисляем общее количество шагов в текущем такте
            double totalStepsInBar = beatsInCurrentBar * stepsPerBeat;
            int wholeSteps = (int)totalStepsInBar;
            double fractionalStep = totalStepsInBar - wholeSteps;
            int stepTicks = (int)(fractionalStep * (TimeLineConverter.TICKS_PER_BEAT / stepsPerBeat));

            // Убеждаемся, что шаги в диапазоне 1-16
            int step = wholeSteps % stepsPerBar;

            string format2 = $"{wholeBars + 1}:{step + 1}:{stepTicks:00}";

            // Выводим оба формата
            _text.text = $"B:B:T: {format1}\nB:S:T: {format2}";
        }
    }
}