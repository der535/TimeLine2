using System;
using UnityEngine;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject
{
    public class TrackObjectData
    {
        public TrackObjectData(double ticksLifeTime, string name, int trackLineIndex, string parentID,
            double startTimeInTicks, double reducedLeft, double reducedRight,
            bool enableResizeLimits = false)
        {
            TimeDurationInTicks = ticksLifeTime;
            Name = name;
            TrackLineIndex = trackLineIndex;
            ParentID = parentID;
            StartTimeInTicks = startTimeInTicks;
            ReducedLeft = reducedLeft;
            ReducedRight = reducedRight;
            EnableResizeLimits = enableResizeLimits;
        }

        public bool EnableResizeLimits;
        public string Name;
        public string ParentID;
        public int TrackLineIndex;
        public double StartTimeInTicks;
        public double TimeDurationInTicks;
        public double ReducedLeft;
        public double ReducedRight;
        public bool IsActive = true;

        public TrackObjectComponents offsetObject;
        public Action<double> OnChangeDuration;

        public void ChangeDurationInTicks(double durationInTicks)
        {
            TimeDurationInTicks = Mathf.Round((float)durationInTicks);
            OnChangeDuration.Invoke(TimeDurationInTicks);
        }

        internal void UpdateDuraction(double newDuractionInTicks)
        {
            var delta = newDuractionInTicks - (TimeDurationInTicks - ReducedRight - ReducedLeft);
            ReducedRight -= delta;
        }

        internal void GroupOffsetTrack(TrackObjectComponents track)
        {
            offsetObject = track;
        }

        internal double GetKeyframeTrackOffset()
        {
            var current = offsetObject;
            int depth = 0;
            const int maxDepth = 50; // защита от зависания

            while (current != null && depth < maxDepth)
            {
                current = current.Data.offsetObject;
                depth++;
            }

            if (depth == maxDepth)
                Debug.Log("Предупреждение: достигнут лимит глубины — возможна циклическая ссылка.");

            // Debug.Log(GetGlobalTicksPosition());
            // Debug.Log(
                // (offsetObject != null ? offsetObject.Data.GetKeyframeTrackOffset() : 0));
            return (offsetObject != null ? offsetObject.Data.GetKeyframeTrackOffset() : 0);
        }

        internal double GetGlobalTicksPosition()
        {
            var start = StartTimeInTicks;
            if (offsetObject != null)
            {
                // Начинаем цепочку логов
                double offsetResult = offsetObject.Data.GetReducedLeft(100);
                double finalResult = Math.Round(start + offsetResult);

                // Debug.Log($"[Root] StartTime: {start} | OffsetResult: {offsetResult} | Final (Rounded): {finalResult}");
                return finalResult;
            }

            return Math.Round(start);
        }

        internal double GetReducedLeft(int depthLimit = 100)
        {
            if (depthLimit <= 0)
            {
                // Debug.LogWarning("Detected potential infinite recursion in offsetObject hierarchy!");
                return StartTimeInTicks;
            }

            // Текущие значения этого звена
            double currentLayerValue = StartTimeInTicks + ReducedLeft;

            if (offsetObject != null)
            {
                // Рекурсивный вызов
                double nextLayerValue = offsetObject.Data.GetReducedLeft(depthLimit - 1);
                double total = currentLayerValue + nextLayerValue;

                // Debug.Log($"[Depth {depthLimit}] Object: {this.GetType().Name} | " +
                // $"StartTime: {StartTimeInTicks} | ReducedLeft: {ReducedLeft} | " +
                // $"Layer Sum: {currentLayerValue} | Accumulated from Parents: {nextLayerValue} | Total: {total}");

                return total;
            }

            // Debug.Log($"[Leaf/End] Object: {this.GetType().Name} | StartTime: {StartTimeInTicks} | ReducedLeft: {ReducedLeft} | Total: {currentLayerValue}");
            return currentLayerValue;
        }

        internal void GroupOffset(double tickOffset)
        {
            // StartTimeInTicks -= tickOffset;
        }

        internal void GroupOffsetNew(double tickOffset)
        {
            StartTimeInTicks -= tickOffset;
        }
    }
}