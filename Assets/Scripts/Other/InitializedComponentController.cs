using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class InitializedComponentController : MonoBehaviour
    {
        [SerializeField] private Main _main;
        
        private List<InitializedComponentData> _initializedComponentData = new();
        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref AddTrackObjectDataEvent data) =>
            {
                Add(data.TrackObjectData.sceneObject.GetComponent<IInitializedComponent>(), data.TrackObjectData.trackObject);
            });
            _gameEventBus.SubscribeTo((ref AddComponentEvent data) =>
            {
                if(data.component is IInitializedComponent initializedComponent)
                    Add(initializedComponent, data.TrackObjectData.trackObject);
            });
            _gameEventBus.SubscribeTo((ref TickExactTimeEvent data) =>
            {
                CheckInitialized(data.Time);
            });
        }

        internal void Add(IInitializedComponent component, TrackObject trackObject)
        {
            _initializedComponentData.Add(new InitializedComponentData(trackObject, component));
        }

        internal void Remove(IInitializedComponent component)
        {
            foreach (var data in _initializedComponentData.ToList())
            {
                if (data.IInitializedComponent == component)
                {
                    _initializedComponentData.Remove(data);
                }
            }
        }

        private void CheckInitialized(double time)
        {
            foreach (var componentData in _initializedComponentData)
            {
                if (time < componentData.TrackObject.StartTimeInTicks && componentData.Initialized == false)
                {
                    componentData.IInitializedComponent?.Initialized();
                    componentData.Initialized = true;
                }
                else if(time > componentData.TrackObject.StartTimeInTicks)
                {
                    componentData.Initialized = false;
                }
            }
        }

        class InitializedComponentData
        {
            public InitializedComponentData(TrackObject trackObject, IInitializedComponent iInitializedComponent)
            {
                TrackObject = trackObject;
                IInitializedComponent = iInitializedComponent;
            }
            
            public TrackObject TrackObject;
            public IInitializedComponent IInitializedComponent;
            public bool Initialized;
        }
    }
}
