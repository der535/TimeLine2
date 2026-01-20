using System;
using System.Reflection;
using EventBus;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.Keyframe
{
    public class KeyframeSelect : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private KeyframeObjectData keyframeObjectData;

        private GameEventBus _gameEventBus;   
        private EventBinder _eventBinder = new EventBinder();
        
        private ThemeStorage _themeStorage;
        
        [Inject]
        private void Construct(GameEventBus gameEventBus, ThemeStorage themeStorage)
        {
            _themeStorage = themeStorage;
            _gameEventBus = gameEventBus;
        }

        private void Awake()
        {
            _eventBinder.Add(_gameEventBus, (ref ThemeChangedEvent data) =>
            {
                image.color = data.Theme.keyframeColor;
            });
            image.color = _themeStorage.value.keyframeColor;
        }

        public void Selected(bool selected)
        {
            if(selected) 
                _gameEventBus.Raise(new SelectKeyframeEvent(keyframeObjectData));
        }

        public void SelectColor(bool selected)
        {
            Debug.Log(selected);
            image.color = selected ? _themeStorage.value.selectedKeyframeColor : _themeStorage.value.keyframeColor;
            print(image.color);
        }

        private void OnDestroy()
        {
            _eventBinder.Dispose();
        }
    }
}
