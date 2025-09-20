using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class InitializedComponentController : MonoBehaviour
    {
        [SerializeField] private Main _main;
        
        private List<InitializedComponentData> _components = new();
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
            _gameEventBus.SubscribeTo((ref AddComponentObjectDataEvent data) =>
            {
                Add(data.InitializedComponent, data.TrackObjectData.trackObject);
            });
        }

        internal void Add(IInitializedComponent component, TrackObject trackObject)
        {
            _components.Add(new InitializedComponentData(trackObject, component));
        }

        private void Update()
        {
            // print(_components.Count);
            foreach (var VARIABLE in _components)
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
