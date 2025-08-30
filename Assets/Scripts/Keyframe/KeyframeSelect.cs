using EventBus;
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
        private bool _selected = false;

        private GameEventBus _gameEventBus;    
        
        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        public void Selected(bool selected)
        {
            if(selected) 
                _gameEventBus.Raise(new SelectKeyframeEvent(keyframeObjectData));
            
            image.color = selected ? new Color(1f, 0.3387191f, 0f) : Color.white;
            
            _selected = selected;
        }
    }
}
