using System.Collections.Generic;
using System.Linq;
using EventBus;
using UnityEngine;
using Zenject;

namespace TimeLine.EventBus.Events.TrackObject
{
    public class SelectObjectController : MonoBehaviour
    {
        [SerializeField] private SelectLock _selectLock;
        private GameEventBus _gameEventBus;
        private List<TrackObjectPacket> _trackObjects = new();

        public List<TrackObjectPacket> SelectObjects => this._trackObjects;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        void Start()
        {
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) => { _trackObjects.Clear(); });
        }

        public void SelectMultiple(TrackObjectPacket trackObject)
        {
            var isMultiple = UnityEngine.Input.GetKey(KeyCode.LeftShift);

            if (_selectLock.IsLocked) return;

            var changed = false;

            if (isMultiple)
            {
                if (!_trackObjects.Contains(trackObject))
                {
                    _trackObjects.Add(trackObject);
                    changed = true;
                }
                else
                {
                    Deselect(trackObject);
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

        public void SelectMultiple(List<TrackObjectPacket> trackObjects)
        {
            
            var changed = false;

            foreach (var trackObject in trackObjects)
            {
                if (!_trackObjects.Contains(trackObject))
                {
                    _trackObjects.Add(trackObject);
                    changed = true;
                }
                else
                {
                    Deselect(trackObject);
                }
            }

            if (changed)
            {
                _gameEventBus.Raise(new SelectObjectEvent(_trackObjects));
            }
        }

        public void SelectNoClear(TrackObjectPacket trackObject)
        {
            if (_selectLock.IsLocked) return;

            if (!_trackObjects.Contains(trackObject))
            {
                _trackObjects.Add(trackObject);
                _gameEventBus.Raise(new SelectObjectEvent(_trackObjects));
            }
        }

        public void Deselect(TrackObjectPacket trackObject)
        {
            _trackObjects.Remove(trackObject);
            if (_trackObjects.Count == 0)
            {
                _gameEventBus.Raise(new DeselectAllObjectEvent());
            }
            else
            {
                _gameEventBus.Raise(new DeselectObjectEvent(trackObject, SelectObjects));
            }
        }

        public void DeselectAll()
        {
            _trackObjects.Clear();
            _gameEventBus.Raise(new DeselectAllObjectEvent());
        }

        public void StartMultipleMove(
            global::TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.TrackObject self)
        {
            foreach (var trackObjectData in _trackObjects.Where(variable => variable.components.TrackObject != self))
            {
                trackObjectData.components.TrackObject.SavePosition();
            }
        }

        public void MultipleMove(global::TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.TrackObject self,
            double ticks)
        {
            if (_trackObjects.Count <= 1 || ticks == 0) return;

            foreach (var trackObjectData in _trackObjects.Where(variable => variable.components.TrackObject != self))
            {
                trackObjectData.components.TrackObject.AddTicksMove(ticks);
            }
        }

        public void MultipleChangeTrackLine(
            global::TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.TrackObject self, int deltaIndex)
        {
            if (_trackObjects.Count <= 1) return;

            foreach (var trackObjectData in _trackObjects.Where(variable => variable.components.TrackObject != self))
            {
                trackObjectData.components.TrackObject.AddLineTrackIndex(deltaIndex);
            }
        }

        public void SaveResizingData(
            global::TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.TrackObject self)
        {
            foreach (var trackObjectData in _trackObjects.Where(variable => variable.components.TrackObject != self))
            {
                trackObjectData.components.TrackObject.SaveResizingData();
            }
        }

        public void MultipleResizingRight(
            global::TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.TrackObject self, double ticks)
        {
            foreach (var trackObjectData in _trackObjects.Where(variable => variable.components.TrackObject != self))
            {
                trackObjectData.components.TrackObject.MultipleRightResize(ticks);
            }
        }

        public void MultipleResizingLeft(
            global::TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject.TrackObject self, double ticks)
        {
            foreach (var trackObjectData in _trackObjects.Where(variable => variable.components.TrackObject != self))
            {
                trackObjectData.components.TrackObject.MultipleLeftResize(ticks);
            }
        }
    }
}