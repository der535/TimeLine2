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
        public List<BezierPoint> selectedPoints = new List<BezierPoint>();
        
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
                bool isShiftHold = UnityEngine.Input.GetKey(KeyCode.LeftShift);
                var point = data.BezierPoint;

                if (!selectedPoints.Contains(point) && !isShiftHold)
                {
                    Deselect();
                    selectedPoints.Add(point);
                }

                if (isShiftHold && !selectedPoints.Contains(point))
                {
                    selectedPoints.Add(point);
                }
            });
        }

        private void Deselect(BezierPoint selectedPoint = null)
        {
            if (selectedPoints == null || selectedPoints.Count <= 0) return;
            foreach (var point in selectedPoints)
            {
                if(selectedPoint == point) continue;
                    point.BezierSelectPoint.Deselect();
            }
            
            selectedPoints.Clear();
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