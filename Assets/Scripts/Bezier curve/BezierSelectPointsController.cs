using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.Bezier;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class BezierSelectPointsController : MonoBehaviour
    {
        List<BezierPoint> selectedPoints = new List<BezierPoint>();
        
        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref AddKeyframeEvent _) => Deselect(), -1);
            _gameEventBus.SubscribeTo((ref RemoveKeyframeEvent _) => Deselect(), -1);
            _gameEventBus.SubscribeTo((ref SelectObjectEvent _) => Deselect(), -1);
            
            _gameEventBus.SubscribeTo((ref BezierSelectPointEvent data) =>
            {
                if(selectedPoints.Contains(data.BezierPoint)) return;
                else
                {
                    Deselect();
                    selectedPoints.Add(data.BezierPoint);
                }
            });
        }

        public void Deselect()
        {
            if (selectedPoints == null || selectedPoints.Count <= 0) return;
            foreach (var point in selectedPoints)
            {
                point.BezierSelectPoint.Deselect();
            }
            
            selectedPoints.Clear();
        }
    }
}