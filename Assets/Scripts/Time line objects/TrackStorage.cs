using System;
using System.Collections.Generic;
using TimeLine.Installers;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class TrackStorage : MonoBehaviour
    {
        [SerializeField] private GameObject track;
        [SerializeField] private RectTransform rootObject;
        [SerializeField] private GameObject buttonAddTrack;
        [Space]
        [SerializeField] private float defaultCountTracks;
        [SerializeField] private float thicknessTrack;
        [Space]
        [SerializeField] private RectTransform timeLineObject;

        [SerializeField] private List<TrackLine> trackLines = new();
        
        internal List<TrackLine> TrackLines => trackLines;
        
        private MainObjects _mainObjects;
        private TimeLineConverter _timeLineConverter;
        

        [Inject]
        private void Construct(
            MainObjects mainObjects, 
            TimeLineConverter timeLineConverter)
        {
            _mainObjects = mainObjects;
            _timeLineConverter = timeLineConverter;
        }
        
        private void Start()
        {
            for (int i = 0; i < defaultCountTracks; i++)
            {
                TrackLine trackRect = Instantiate(track, rootObject).GetComponent<TrackLine>();
                trackRect.RectTransform.sizeDelta =
                    new Vector2(_mainObjects.CanvasRectTransform.sizeDelta.x, thicknessTrack);
                trackLines.Add(trackRect);
            }
        }

        public TrackLine CheckTracks(TrackObject trackObject)
        {
            if (trackLines.Count <= 1) return trackObject.TrackLine;

            Vector2 cursorPosition = new Vector2(_timeLineConverter.CursorPosition().x,
                _timeLineConverter.CursorPosition().y +
                (_mainObjects.CanvasRectTransform.sizeDelta.y / 2 - timeLineObject.sizeDelta.y));

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