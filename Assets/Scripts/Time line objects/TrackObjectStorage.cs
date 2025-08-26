using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class TrackObjectStorage : MonoBehaviour
    {
        private readonly List<TrackObjectData> _trackObjects = new();

        public TrackObjectData _selectedObject;
        
        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo<SmoothTimeEvent>(ActiveSceneObject);
        }

        private void ActiveSceneObject(ref SmoothTimeEvent smoothTimeEvent)
        {
            foreach (var trackObject in _trackObjects)
            {
                trackObject.sceneObject.SetActive(trackObject.trackObject.StartTime <= smoothTimeEvent.Time &&
                                                  trackObject.trackObject.TimeDuraction + trackObject.trackObject.StartTime  >
                                                  smoothTimeEvent.Time);
            }
        }
        
        internal void Add(GameObject sceneObject, TrackObject selectedObject, Branch branch)
        {
            TrackObjectData trackObjectData = new TrackObjectData(sceneObject, selectedObject, branch);
            _gameEventBus.Raise(new AddTrackObjectDataEvent(trackObjectData));
            _trackObjects.Add(trackObjectData);
        }

        internal void Remove(TrackObjectData trackObjectData)
        {
            _trackObjects.Remove(trackObjectData);
        }

        internal TrackObjectData GetTrackObjectData(GameObject gObject)
        {
            return _trackObjects.FirstOrDefault(trackObject => trackObject.sceneObject == gObject);
        }

        public void UpdatePositionSelectedTrackObject()
        {
            _gameEventBus.Raise(new DragTrackObjectEvent(_selectedObject));
        }

        public void SelectObject(TrackObject selectedObject)
        {
            foreach (var trackObject in _trackObjects)
            {
                if (trackObject.trackObject == selectedObject)
                {
                    _gameEventBus.Raise(new SelectTrackObjectEvent(trackObject));
                    _selectedObject = trackObject;
                }
            }
            
            
            foreach (var trackObject in _trackObjects)
            {
                trackObject.trackObject.Deselect();
            }
        }

        public void ResetSelection()
        {
            _selectedObject?.trackObject.Deselect();
            _selectedObject = null;
            _gameEventBus.Raise(new SelectTrackObjectEvent(_selectedObject));
        }
    }
    
    [Serializable]
    public class TrackObjectData
    {
        public GameObject sceneObject;
        public TrackObject trackObject;
        public Branch branch;

        public TrackObjectData(GameObject sceneObject, TrackObject trackObject, Branch branch)
        {
            this.sceneObject = sceneObject;
            this.trackObject = trackObject;
            this.branch = branch;
        }
    }
}
