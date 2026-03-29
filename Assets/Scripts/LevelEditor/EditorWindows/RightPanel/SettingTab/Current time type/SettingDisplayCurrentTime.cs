using System;
using System.Collections.Generic;
using System.Globalization;
using EventBus;
using EventBus.Events.Settings;
using TimeLine.LevelEditor.Core;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Tabs.SettingTab.Current_time_type
{
    public class SettingDisplayCurrentTime : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;

        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            List<string> options = new List<string>();
            options.Add("ticks");
            options.Add("bar:beat:tick");
            options.Add("bar:step:tick");

            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.onValueChanged.AddListener(_ => { _gameEventBus.Raise(new ChangeEditorSettingsEvent()); });
        }

        public string GetSettingDisplayCurrentTime() => dropdown.options[dropdown.value].text;

        public void SetSettingDisplayCurrentTime(string value)
        {
            for (var index = 0; index < dropdown.options.Count; index++)
            {
                var option = dropdown.options[index];
                if (option.text == value)
                {
                    dropdown.value = index;
                }
            }
        }

        public string ConvertTicksToFormat(double currentTimeInTicks)
        {
            // Константы для преобразования
            const double beatsPerBar = 4.0; // обычно 4 доли в такте
            const int stepsPerBeat = 4; // 4 шага на долю (1/16 ноты)
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

            switch (dropdown.options[dropdown.value].text)
            {
                case "ticks":
                    return Math.Floor(currentTimeInTicks).ToString(CultureInfo.InvariantCulture);
                case "bar:beat:tick":
                    // Формат 1: bar:beat:tick (такты:доли:тики)
                    return $"{wholeBars + 1}:{wholeBeats + 1}:{ticks:00}";
                case "bar:step:tick":
                    // Формат 2: bar:step:tick (такты:шаги:тики)
                    // Вычисляем общее количество шагов в текущем такте
                    double totalStepsInBar = beatsInCurrentBar * stepsPerBeat;
                    int wholeSteps = (int)totalStepsInBar;
                    double fractionalStep = totalStepsInBar - wholeSteps;
                    int stepTicks = (int)(fractionalStep * (TimeLineConverter.TICKS_PER_BEAT / stepsPerBeat));

                    // Убеждаемся, что шаги в диапазоне 1-16
                    int step = wholeSteps % stepsPerBar;
                    return $"{wholeBars + 1}:{step + 1}:{stepTicks:00}";
                default: return string.Empty;
            }
        }

        public double ConvertFromFormatToTicks(string inputText)
        {
            const double beatsPerBar = 4.0;
            const int stepsPerBeat = 4;
            const int ticksPerBeat = (int)TimeLineConverter.TICKS_PER_BEAT;
            const int stepsPerBar = (int)(beatsPerBar * stepsPerBeat); // 16
            const int ticksPerStep = ticksPerBeat / stepsPerBeat; // обычно 120, если TICKS_PER_BEAT = 480

            if (string.IsNullOrWhiteSpace(inputText))
                return 0;

            // Убираем лишние пробелы
            inputText = inputText.Trim();

            try
            {
                switch (dropdown.options[dropdown.value].text)
                {
                    case "ticks":
                        // Просто парсим как число — даже если введено "01:02", это ошибка, но мы попробуем как число
                        if (double.TryParse(inputText, NumberStyles.Float, CultureInfo.InvariantCulture,
                                out double ticks))
                            return Math.Max(0, ticks);
                        return 0;

                    case "bar:beat:tick":
                    {
                        string[] parts = inputText.Split(':');
                        int bar, beat, tick;

                        if (parts.Length == 1)
                        {
                            // Только одно число → интерпретируем как bar (минимум 1)
                            bar = Math.Max(1, int.Parse(parts[0], CultureInfo.InvariantCulture));
                            beat = 1;
                            tick = 0;
                        }
                        else if (parts.Length == 2)
                        {
                            bar = Math.Max(1, int.Parse(parts[0], CultureInfo.InvariantCulture));
                            beat = Math.Max(1, int.Parse(parts[1], CultureInfo.InvariantCulture));
                            tick = 0;
                        }
                        else if (parts.Length == 3)
                        {
                            bar = Math.Max(1, int.Parse(parts[0], CultureInfo.InvariantCulture));
                            beat = Math.Max(1, int.Parse(parts[1], CultureInfo.InvariantCulture));
                            tick = Math.Max(0, int.Parse(parts[2], CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            return 0;
                        }

                        // Нормализуем избыточные доли → переносим в такты
                        bar += (beat - 1) / (int)beatsPerBar;
                        beat = (beat - 1) % (int)beatsPerBar + 1;

                        // Нормализуем тики → переносим в доли
                        beat += tick / ticksPerBeat;
                        tick = tick % ticksPerBeat;

                        // Повторно нормализуем beat → bar, если tick дал переполнение
                        bar += (beat - 1) / (int)beatsPerBar;
                        beat = (beat - 1) % (int)beatsPerBar + 1;

                        return ((bar - 1) * beatsPerBar + (beat - 1)) * ticksPerBeat + tick;
                    }

                    case "bar:step:tick":
                    {
                        string[] parts = inputText.Split(':');
                        int bar, step, tick;

                        if (parts.Length == 1)
                        {
                            bar = Math.Max(1, int.Parse(parts[0], CultureInfo.InvariantCulture));
                            step = 1;
                            tick = 0;
                        }
                        else if (parts.Length == 2)
                        {
                            bar = Math.Max(1, int.Parse(parts[0], CultureInfo.InvariantCulture));
                            step = Math.Max(1, int.Parse(parts[1], CultureInfo.InvariantCulture));
                            tick = 0;
                        }
                        else if (parts.Length == 3)
                        {
                            bar = Math.Max(1, int.Parse(parts[0], CultureInfo.InvariantCulture));
                            step = Math.Max(1, int.Parse(parts[1], CultureInfo.InvariantCulture));
                            tick = Math.Max(0, int.Parse(parts[2], CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            return 0;
                        }

                        // Нормализуем шаги → переносим в такты
                        bar += (step - 1) / stepsPerBar;
                        step = (step - 1) % stepsPerBar + 1;

                        // Нормализуем тики → переносим в шаги
                        step += tick / ticksPerStep;
                        tick = tick % ticksPerStep;

                        // Повторно нормализуем step → bar
                        bar += (step - 1) / stepsPerBar;
                        step = (step - 1) % stepsPerBar + 1;

                        // Переводим в биты
                        double beats = (step - 1) / (double)stepsPerBeat;
                        return ((bar - 1) * beatsPerBar + beats) * ticksPerBeat + tick;
                    }

                    default:
                        return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}