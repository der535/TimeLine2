using System.Collections.Generic;
using System.Linq;
using EventBus;
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
                Add(data.InitializedComponent, data.TrackObjectData.trackObject);
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

        private void Update()
        {
            // print(_components.Count);
            foreach (var VARIABLE in _initializedComponentData)
            {
                if (_main.TicksCurrentTime() <= VARIABLE.TrackObject.StartTimeInTicks && VARIABLE.Initialized == false)
                {
                    VARIABLE.IInitializedComponent?.Initialized();
                    VARIABLE.Initialized = true;
                }
                else if(_main.TicksCurrentTime() > VARIABLE.TrackObject.StartTimeInTicks)
                {
                    VARIABLE.Initialized = false;
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
