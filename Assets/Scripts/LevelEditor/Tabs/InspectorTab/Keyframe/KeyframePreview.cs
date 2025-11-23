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
                _keyframe = data.Keyframe.Keyframe;
                text.text = $"Time: {data.Keyframe.Keyframe.Ticks.ToString()}, Value: {data.Keyframe.Keyframe.GetData().GetValue()}";
            });
        }

        private void Update() //todo Удалить потом
        {
            if(_keyframe != null)
                text.text = $"Time: {_keyframe.Ticks.ToString()}, Value: {_keyframe.GetData().GetValue()}";
        }
    }
}
