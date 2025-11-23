using System.Globalization;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class KeyframeEditWindow : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _timeInpute;
        [SerializeField] private TMP_InputField _valueInpute;
        
        private GameEventBus _gameEventBus;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo<SelectKeyframeEvent>(Setup);
        }

        private void Setup(ref SelectKeyframeEvent data)
        {
            _timeInpute.onEndEdit.RemoveAllListeners();
            _valueInpute.onEndEdit.RemoveAllListeners();
            
            _timeInpute.text = data.Keyframe.Keyframe.Ticks.ToString(CultureInfo.InvariantCulture);
            _valueInpute.text = data.Keyframe.Keyframe.GetData().GetValue().ToString();
            var @event = data;
            
            _timeInpute.onEndEdit.AddListener(arg0 => @event.Keyframe.Keyframe.Ticks = int.Parse(arg0));
            _valueInpute.onEndEdit.AddListener(arg0 => @event.Keyframe.Keyframe.GetData().SetValue((float)int.Parse(arg0)));
        }
    }
}
