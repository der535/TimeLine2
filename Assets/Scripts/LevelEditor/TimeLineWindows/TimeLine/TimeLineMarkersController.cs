using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.Input;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine
{
    public class TimeLineMarkersController : MonoBehaviour
    {
        [SerializeField] private RectTransform rootObject;
        [Space]
        [SerializeField] private TimeLineMarker markerPrefab;
        
        private List<TimeLineMarker> markers = new();
        private DiContainer _container;
        private GameEventBus _eventBus;
        
        [Inject]
        private void Constructor(DiContainer container, GameEventBus eventBus)
        {
            _container = container;
            _eventBus = eventBus;
        }

        private void Start()
        {
            _eventBus.SubscribeTo((ref PanEvent panEvent) => UpdatePosition());
            _eventBus.SubscribeTo((ref ScrollTimeLineEvent panEvent) => UpdatePosition());
        }

        internal TimeLineMarker AddMarker(double time, Color color)
        {
            TimeLineMarker marker = _container.InstantiatePrefab(markerPrefab, rootObject).GetComponent<TimeLineMarker>();
            marker.Setup(time, color);
            markers.Add(marker);
            UpdatePosition();
            return marker;
        }
        
        internal void RemoveMarker(TimeLineMarker marker)
        {
            markers.Remove(marker);
            Destroy(marker.gameObject);
        }

        private void UpdatePosition()
        {
            foreach (var marker in markers)
            {
                marker.UpdatePosition();
            }
        }
    }
}
