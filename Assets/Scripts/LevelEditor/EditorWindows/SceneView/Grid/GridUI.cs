using System;
using TimeLine.TimeLine;
using UnityEngine;

namespace TimeLine
{
    public class GridUI : MonoBehaviour
    {
        [Tooltip("Grid size in beats (0.25 = 1/4, 0.125 = 1/8, etc.)")]
        private float gridSize = 0.25f;

        public float GridSize
        {
            get => gridSize;
            set => gridSize = value;
        }

        // Конвертирует размер сетки в тики для текущего BPM
        public double GetGridSizeInTicks()
        {
            return gridSize * TimeLineConverter.TICKS_PER_BEAT;
        }

        // Округляет позицию в тиках до ближайшей сетки
        public double RoundTicksToGrid(double ticks)
        {
            double gridSizeInTicks = GetGridSizeInTicks();
            return Math.Round(ticks / gridSizeInTicks) * gridSizeInTicks;
        }

        // Округляет позицию в секундах до сетки
        public float RoundTimeToGrid(float time)
        {
            double ticks = TimeLineConverter.Instance.SecondsToTicks(time);
            double roundedTicks = RoundTicksToGrid(ticks);
            return (float)TimeLineConverter.Instance.TicksToSeconds(roundedTicks);
        }

        // Округляет позицию в пикселях до сетки
        public float RoundAnchorPositionToGrid(float position, float pan)
        {
            double ticksPerPixel = TimeLineConverter.TICKS_PER_BEAT / pan;
            double ticks = position * ticksPerPixel;
            double roundedTicks = RoundTicksToGrid(ticks);
            return (float)(roundedTicks / ticksPerPixel);
        }
        
        // public double RoundTimeToGridInTicks(double ticks)
        // {
        //     double seconds = ticks * (60.0 / (main.MusicData.bpm * TimeLineConverter.TICKS_PER_BEAT));
        //     double roundedSeconds = RoundTimeToGrid((float)seconds);
        //     return roundedSeconds * (main.MusicData.bpm * TimeLineConverter.TICKS_PER_BEAT / 60.0);
        // }
    }
}