using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.Installers;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
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
        private GameEventBus _gameEventBus;
        

        [Inject]
        private void Construct(
            MainObjects mainObjects, 
            TimeLineConverter timeLineConverter,
            GameEventBus gameEventBus)
        {
            _mainObjects = mainObjects;
            _timeLineConverter = timeLineConverter;
            _gameEventBus = gameEventBus;
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
        
        public void RemoveLine()
        {
            var go = trackLines[^1];
            _gameEventBus.Raise(new RemoveTrackLineEvent(go));
            trackLines.Remove(go);
            Destroy(go.gameObject);
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

        public TrackLine CheckTracks(TrackObject trackObject)
        {
            if (trackLines.Count <= 1) return trackObject.TrackLine;


            
            Vector2 cursorPosition = new Vector2(_timeLineConverter.CursorPosition().x,
                _timeLineConverter.CursorPosition().y +
                (_mainObjects.CanvasRectTransform.sizeDelta.y / 2 - scrollViewObject.sizeDelta.y)-trackLinesContent.anchoredPosition.y);

            float minDistance = float.MaxValue;
            TrackLine closestTrack = null;
            int closestIndex = -1;

            for (int i = 0; i < trackLines.Count; i++)
            {
                TrackLine line = trackLines[i];
                // Рассчитываем расстояние по Y между центром трека и курсором
                float distance = Math.Abs(line.RectTransform.localPosition.y - cursorPosition.y);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTrack = line;
                    closestIndex = i;
                }
            }

            return closestTrack;
        }
    }
}