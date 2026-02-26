using System;
using EventBus;
using TimeLine.EventBus.Events.Misc;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.LevelEditor.InspectorTab.Parameter
{
    public class GetParameter : MonoBehaviour
    {
        [SerializeField] private Button button;
        private InspectableParameter _parameter;
        private MapParameterComponen _mapParameterComponen;
        private GameEventBus _gameEventBus;
        private EventBinder eventBinder;

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

        public void Setup(InspectableParameter parameter, TrackObjectPacket trackObjectPacket,
            BaseParameterComponent component)
        {
            _parameter = parameter;
            _mapParameterComponen =
                new MapParameterComponen(trackObjectPacket.sceneObjectID, component.GetID(), parameter.Id);
        }

        public void Get()
        {
            _gameEventBus.Raise(new GetParameterEvent((_parameter, _mapParameterComponen)));
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
    public string ComponentID;
    public string ParameterID;

    public MapParameterComponen(string sceneObjectID, string componentID, string parameterID)
    {
        SceneObjectID = sceneObjectID;
        ComponentID = componentID;
        ParameterID = parameterID;
    }
}