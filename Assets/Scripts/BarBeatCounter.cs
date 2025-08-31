using System;
using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.Installers;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class BarBeatCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        private Main _main;
        private GameEventBus _gameEventBus;

        private const float SecondsInMunit = 60f;
        private double _oldBeat;

        [Inject]
        private void Construct(
            Main main,
            GameEventBus gameEventBus)
        {
            _main = main;
            _gameEventBus = gameEventBus;
        }

        public void Awake()
        {
            _gameEventBus.SubscribeTo<ExactTimeEvent>(OnTimeChangedUnSmooth);
            _gameEventBus.SubscribeTo<TickExactTimeEvent>(Calculate);
        }

        public void OnTimeChangedUnSmooth(ref ExactTimeEvent timeEvent)
        {
            float currentTime = timeEvent.Time;
            float beatInSeconds = SecondsInMunit / _main.MusicDataSo.bpm;
            double currentBeat = Math.Ceiling((currentTime) / beatInSeconds);
            if (currentBeat != _oldBeat)
            {
                _oldBeat = currentBeat;
                _gameEventBus.Raise(new BeatEvent((int)currentBeat));
            }
        }

        private void Calculate(ref TickExactTimeEvent timeEvent)
        {
            double currentTimeInTicks = timeEvent.Time;
    
            // Константы для преобразования
            const double ticksPerBeat = 96.0; // TICKS_PER_BEAT
            const double beatsPerBar = 4.0;   // обычно 4 доли в такте
            const int stepsPerBeat = 4;       // 4 шага на долю (1/16 ноты)
            const int stepsPerBar = (int)(beatsPerBar * stepsPerBeat); // 16 шагов в такте
    
            // Вычисляем компоненты времени
            double totalBeats = currentTimeInTicks / ticksPerBeat;
            double bars = totalBeats / beatsPerBar;
    
            // Целая часть - такты
            int wholeBars = (int)bars;
    
            // Дробная часть - доли и тики
            double fractionalBar = bars - wholeBars;
            double beatsInCurrentBar = fractionalBar * beatsPerBar;
    
            int wholeBeats = (int)beatsInCurrentBar;
            double fractionalBeat = beatsInCurrentBar - wholeBeats;
            int ticks = (int)(fractionalBeat * ticksPerBeat);
    
            // Формат 1: bar:beat:tick (такты:доли:тики)
            string format1 = $"{wholeBars + 1}:{wholeBeats + 1}:{ticks:00}";
    
            // Формат 2: bar:step:tick (такты:шаги:тики)
            // Вычисляем общее количество шагов в текущем такте
            double totalStepsInBar = beatsInCurrentBar * stepsPerBeat;
            int wholeSteps = (int)totalStepsInBar;
            double fractionalStep = totalStepsInBar - wholeSteps;
            int stepTicks = (int)(fractionalStep * (ticksPerBeat / stepsPerBeat));
    
            // Убеждаемся, что шаги в диапазоне 1-16
            int step = wholeSteps % stepsPerBar;
    
            string format2 = $"{wholeBars + 1}:{step + 1}:{stepTicks:00}";
    
            // Выводим оба формата
            _text.text = $"B:B:T: {format1}\nB:S:T: {format2}";
        }
    }
}