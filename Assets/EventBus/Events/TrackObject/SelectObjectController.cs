using System.Collections.Generic;
using System.Linq;
using EventBus;
using UnityEngine;
using Zenject;

namespace TimeLine.EventBus.Events.TrackObject
{
    public class SelectObjectController : MonoBehaviour
    {
        private GameEventBus _gameEventBus;
        private List<TrackObjectData> _trackObjects = new List<TrackObjectData>();
        
        public List<TrackObjectData> SelectObjects => this._trackObjects;
        
        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        void Start()
        {
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) =>
            {
                _trackObjects.Clear();
            });
        }
        public void Select(TrackObjectData trackObject, bool isMultiple)
        {
            var changed = false;

            if (isMultiple)
            {
                if (!_trackObjects.Contains(trackObject))
                {
                    _trackObjects.Add(trackObject);
                    changed = true;
                }
            }
            else
            {
                if (!_trackObjects.Contains(trackObject))
                {
                    _trackObjects.Clear();
                    _trackObjects.Add(trackObject);
                    changed = true;
                }
            }

            if (changed)
            {
                _gameEventBus.Raise(new SelectObjectEvent(_trackObjects));
            }
        }

        public void StartMultipleMove(global::TimeLine.TrackObject self)
        {
            foreach (var trackObjectData in _trackObjects.Where(variable => variable.trackObject != self))
            {
                trackObjectData.trackObject.SavePosition();
            }
        }

        public void MultipleMove(global::TimeLine.TrackObject self, double ticks)
        {
            if (_trackObjects.Count <= 1 || ticks == 0) return;
            
            foreach (var trackObjectData in _trackObjects.Where(variable => variable.trackObject != self))
            {
                trackObjectData.trackObject.AddTicksMove(ticks);
            }
        }

        public void SaveResizingData(global::TimeLine.TrackObject self)
        {
            foreach (var trackObjectData in _trackObjects.Where(variable => variable.trackObject != self))
            {
                trackObjectData.trackObject.SaveResizingData();
            }
        }
        
        public void MultipleResizingRight(global::TimeLine.TrackObject self, double ticks)
        {
            foreach (var trackObjectData in _trackObjects.Where(variable => variable.trackObject != self))
            {
                trackObjectData.trackObject.MultipleRightResize(ticks);
            }
        }
        
        public void MultipleResizingLeft(global::TimeLine.TrackObject self, double ticks)
        {
            foreach (var trackObjectData in _trackObjects.Where(variable => variable.trackObject != self))
            {
                trackObjectData.trackObject.MultipleLeftResize(ticks);
            }
        }
    }
}