using System;
using EventBus;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.EventBus.Events.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class DynamicTransformСomponent : MonoBehaviour
    {
        public BoolParameter ComponentActive = new("Component active", true);
        
        public Vector2Parameter DynamicXPosition = new("DynamicXPosition", "startValue", "speed", Vector2.zero);
        public BoolParameter DynamicXPositionActive = new("p-x", false);
        public Vector2Parameter DynamicYPosition = new("DynamicYPosition", "startValue", "speed", Vector2.zero);
        public BoolParameter DynamicYPositionActive = new("p-y", false);
        
        public Vector2Parameter DynamicXRotation = new("DynamicXRotation", "startValue", "speed", Vector2.zero);
        public BoolParameter DynamicXRotationActive = new("p-x", false);
        public Vector2Parameter DynamicYRotation = new("DynamicYRotation", "startValue", "speed", Vector2.zero);
        public BoolParameter DynamicYRotationActive = new("p-y", false);
        public Vector2Parameter DynamicZRotation = new("DynamicZRotation", "startValue", "speed", Vector2.zero);
        public BoolParameter DynamicZRotationActive = new("p-z", false);
        
        public Vector2Parameter DynamicXScale = new("DynamicXScale", "startValue", "speed", Vector2.zero);
        public BoolParameter DynamicXScaleActive = new("p-x", false);
        public Vector2Parameter DynamicYScale = new("DynamicYScale", "startValue", "speed", Vector2.zero);
        public BoolParameter DynamicYScaleActive = new("p-y", false);

        private TransformComponent _component;
        private GameEventBus _gameEventBus;
        private TrackObjectStorage _trackObjectStorage;
        
        private TrackObject _trackObject;
        private Main _main;

        // Используем правильный делегат EventHandler вместо Action
        private GenericEventBus.GenericEventBus<IEvent>.EventHandler<TickSmoothTimeEvent> _tickEventHandler;

        [Inject]
        private void Construct(GameEventBus eventBus, TrackObjectStorage trackObjectStorage, Main main)
        {
            _gameEventBus = eventBus;
            _trackObjectStorage = trackObjectStorage;
            _main = main;
        }
        
        private void Start()
        {
            // Инициализируем правильный делегат EventHandler
            _tickEventHandler = (ref TickSmoothTimeEvent data) => UpdateValues(data.Time);
            
            // Подписываемся с явным указанием типа
            _gameEventBus.SubscribeTo(_tickEventHandler);
            
            _component = GetComponent<TransformComponent>();
            // _trackObjectStorage.GetTrackObjectData(gameObject);
        }

        private void OnDestroy()
        {
            // Отписываемся при уничтожении объекта
            if (_gameEventBus != null && _tickEventHandler != null)
            {
                _gameEventBus.UnsubscribeFrom(_tickEventHandler);
            }
        }

        private void UpdateValues(double ticks)
        {
            if(ComponentActive.Value == false || gameObject.activeSelf == false) return;
            
            // Конвертируем тики в секунды с учетом BPM
            double seconds = TicksToSeconds(ticks, _main.MusicDataSo.bpm);
            
            if(DynamicXPositionActive.Value)
                _component.XPosition.Value = (float)(DynamicXPosition.Value.x + seconds * DynamicXPosition.Value.y);
            if(DynamicYPositionActive.Value)
                _component.YPosition.Value = (float)(DynamicYPosition.Value.x + seconds * DynamicYPosition.Value.y);
            if(DynamicXRotationActive.Value)
                _component.XRotation.Value = (float)(DynamicXRotation.Value.x + seconds * DynamicXRotation.Value.y);
            if(DynamicYRotationActive.Value)
                _component.YRotation.Value = (float)(DynamicYRotation.Value.x + seconds * DynamicYRotation.Value.y);
            if(DynamicZRotationActive.Value)
                _component.ZRotation.Value = (float)(DynamicZRotation.Value.x + seconds * DynamicZRotation.Value.y);
            if(DynamicXScaleActive.Value)
                _component.XScale.Value = (float)(DynamicXScale.Value.x + seconds * DynamicXScale.Value.y);
            if(DynamicYScaleActive.Value)
                _component.YScale.Value = (float)(DynamicYScale.Value.x + seconds * DynamicYScale.Value.y);
        }
        
        private double TicksToSeconds(double ticks, double bpm)
        {
            // Конвертация тиков в секунды: ticks * (60 / (bpm * TICKS_PER_BEAT))
            return ticks * (60.0 / (bpm * Main.TICKS_PER_BEAT));
        } 
    }
}