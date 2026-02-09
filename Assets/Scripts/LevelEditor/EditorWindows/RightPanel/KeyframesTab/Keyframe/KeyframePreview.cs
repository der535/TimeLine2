using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class KeyframePreview : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private GameEventBus _gameEventBus;
        private Keyframe.Keyframe _keyframe;
        
        [Inject]
        void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        private void Start()
        {
            _gameEventBus.SubscribeTo((ref SelectKeyframeEvent data) =>
            {
                _keyframe = data.Keyframe;
                text.text = $"Time: {data.Keyframe.Ticks.ToString()}, Value: {data.Keyframe.GetData().GetValue()}";
            });
            
            _gameEventBus.SubscribeTo((ref DeselectAllKeyframeEvent data) =>
            {
                text.text = string.Empty;
            });
        }
    }
}
