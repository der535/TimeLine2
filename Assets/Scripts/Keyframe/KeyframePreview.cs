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
        
        [Inject]
        void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        private void Start()
        {
            _gameEventBus.SubscribeTo((ref SelectKeyframeEvent data) =>
            {
                text.text = $"Time: {data.Keyframe.Keyframe.ticks.ToString()}, Value: {data.Keyframe.Keyframe.GetData().GetValue()}";
            });
        }


    }
}
