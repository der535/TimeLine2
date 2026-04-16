using System;
using EventBus;
using TimeLine.EventBus.Events.Misc;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.LevelEditor.InspectorTab.Parameter
{
    public class GetParameter : MonoBehaviour
    {
        [SerializeField] private Button button;
        private MapParameterComponen _mapParameterComponen;
        private GameEventBus _gameEventBus;
        private EventBinder eventBinder;
        private TrackObjectPacket _trackObjectPacket;

        [Inject]
        private void Construct(GameEventBus parameter)
        {
            _gameEventBus = parameter;
        }

        private void Start()
        {
            button.onClick.AddListener(() => { Get(); });
            button.gameObject.SetActive(false);
            eventBinder = new EventBinder();
            eventBinder.Add(_gameEventBus, (ref ListeningParameterEvent _) => button.gameObject.SetActive(true));
            eventBinder.Add(_gameEventBus, (ref StopListeningParameterEvent _) => button.gameObject.SetActive(false));
        }

        public void Setup(TrackObjectPacket trackObjectPacket, string parameterID)
        {
            _trackObjectPacket = trackObjectPacket;
            _mapParameterComponen =
                new MapParameterComponen(trackObjectPacket.sceneObjectID, parameterID);
        }

        public void Get()
        {
            _gameEventBus.Raise(new GetParameterEvent(_mapParameterComponen, _trackObjectPacket));
            _gameEventBus.Raise(new StopListeningParameterEvent());
        }

        private void OnDestroy()
        {
            eventBinder.Dispose();
        }
    }
}
[Serializable]
public class MapParameterComponen
{
    public string SceneObjectID;
    public string ParameterID;
    
    public Entity Entity {get; private set;}

    public MapParameterComponen(string sceneObjectID, string parameterID)
    {
        SceneObjectID = sceneObjectID;
        ParameterID = parameterID;
    }
}