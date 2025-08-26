using EventBus;
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

        [Inject]
        private void Construct(GameEventBus eventBus, TrackObjectStorage trackObjectStorage)
        {
            _gameEventBus = eventBus;
            _trackObjectStorage = trackObjectStorage;
        }
        
        private void Start()
        {
            _gameEventBus.SubscribeTo((ref SmoothTimeEvent data) => UpdateValues(data.Time));
            _component = GetComponent<TransformComponent>();
            _trackObjectStorage.GetTrackObjectData(gameObject);
        }

        private void UpdateValues(float time)
        {
            if(ComponentActive.Value == false || gameObject.activeSelf == false) return;
            
            if(DynamicXPositionActive.Value)
                _component.XPosition.Value = DynamicXPosition.Value.x + time * DynamicXPosition.Value.y;
            if(DynamicYPositionActive.Value)
                _component.YPosition.Value = DynamicYPosition.Value.x + time * DynamicYPosition.Value.y;
            if(DynamicXRotationActive.Value)
                _component.XRotation.Value = DynamicXRotation.Value.x + time * DynamicXRotation.Value.y;
            if(DynamicYRotationActive.Value)
                _component.YRotation.Value = DynamicYRotation.Value.x + time * DynamicYRotation.Value.y;
            if(DynamicZRotationActive.Value)
                _component.ZRotation.Value = DynamicZRotation.Value.x + time * DynamicZRotation.Value.y;
            if(DynamicXScaleActive.Value)
                _component.XScale.Value = DynamicXScale.Value.x + time * DynamicXScale.Value.y;
            if(DynamicYScaleActive.Value)
                _component.YScale.Value = DynamicYScale.Value.x + time * DynamicYScale.Value.y;
        }
    }
}
