using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.Installers;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class TrackStorage : MonoBehaviour
    {
        [SerializeField] private GameObject track;
        [SerializeField] private RectTransform rootObject;
        [SerializeField] private GameObject buttonAddTrack;
        [Space]
        [SerializeField] private float thicknessTrack;
        [Space]
        [SerializeField] private RectTransform timeLineObject;
        [SerializeField] private RectTransform scrollViewObject;
        [SerializeField] private RectTransform trackLinesContent;
        [SerializeField] private List<TrackLine> trackLines = new();
        [SerializeField] private int linesCount;
        
        internal List<TrackLine> TrackLines => trackLines;
        
        private MainObjects _mainObjects;
        private TimeLineConverter _timeLineConverter;
        
        [Inject]
        private void Construct(
            MainObjects mainObjects, 
            TimeLineConverter timeLineConverter,
            GameEventBus gameEventBus)
        {
            _mainObjects = mainObjects;
            _timeLineConverter = timeLineConverter;
        }

        private void Start()
        {
            while (TrackLines.Count < linesCount)
            {
                AddLine();
            }
        }

        public void AddLine()
        {
            TrackLine trackRect = Instantiate(track, rootObject).GetComponent<TrackLine>();
            trackRect.RectTransform.sizeDelta =
                new Vector2(_mainObjects.CanvasRectTransform.sizeDelta.x, thicknessTrack);
            trackLines.Add(trackRect);
        }

        internal int GetTrackLineIndex(TrackLine trackLine)
        {
            return trackLines.IndexOf(trackLine);
        }

        internal TrackLine GetTrackLineByIndex(int index)
        {
            while (index > trackLines.Count-1)
            {
                AddLine();
            }
            
            return trackLines[index];
        }

        public int GetIndex(TrackLine trackLine)
        {
            for (int i = 0; i < trackLines.Count; i++)
            {
                if(trackLine == trackLines[i]) return i;
            }

            return -1;
        }

        public TrackLine GetTrackLine(int index)
        {
            if (index < 0) return trackLines[0];
            if (index >= trackLines.Count) return trackLines[^1];
            return trackLines[index];
        }

        public int CheckTracks(int index)
        {
            if (trackLines.Count <= 1) return index;
            
            Vector2 cursorPosition = new Vector2(_timeLineConverter.CursorPosition().x,
                _timeLineConverter.CursorPosition().y +
                (_mainObjects.CanvasRectTransform.sizeDelta.y / 2 - scrollViewObject.sizeDelta.y)-trackLinesContent.anchoredPosition.y);

            float minDistance = float.MaxValue;
            int closestIndex = -1;

            for (int i = 0; i < trackLines.Count; i++)
            {
                TrackLine line = trackLines[i];
                // Рассчитываем расстояние по Y между центром трека и курсором
                float distance = Math.Abs(line.RectTransform.localPosition.y - cursorPosition.y);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }
    }
}