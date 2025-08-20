using System;
using UnityEngine;

namespace TimeLine
{
    public class GridUI : MonoBehaviour
    {
        [SerializeField] private TimeLineSettings _timeLineSettings;
        [SerializeField] private Main main;
        [Range(0, 1000)] [SerializeField] private float gridSize;

        public float GridSize
        {
            get => gridSize;
            set => gridSize = value;
        }

        public float RoundBeatPositionToGrid(float time)
        {
            var calculatedTime = time * 1000;
            calculatedTime = (float)Math.Round(calculatedTime / gridSize) * gridSize;
            calculatedTime /= 1000;
            return calculatedTime;
        }

        public float RoundTimeToGrid(float time)
        {
            float beatInSecond = 60 / main.MusicDataSo.bpm;
            float calculatedGridMultipluer = gridSize / 1000;
            float beatInSecondMultiplierd = calculatedGridMultipluer * beatInSecond;
            float calculatedTime = (float)Math.Round(time / beatInSecondMultiplierd) * beatInSecondMultiplierd;

            // print($"beatInSecond {beatInSecond} === time {time} === calculatedTime {calculatedTime}");

            return calculatedTime;
        }

        public float RoundAnchorPositionToGrid(float position)
        {
            float grid = (_timeLineSettings.DistanceBetweenBeatLines * (gridSize / 1000));
            float calculatedPosition = (float)Math.Round(position / grid) * grid;
            return calculatedPosition;
        }
    }
}