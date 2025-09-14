using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class DisplayTrackDuraction : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField;

        private GameEventBus _gameEventBus;
        private TrackObjectStorage _trackObjectStorage;
        
        [Inject]
        private void Construct(GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage)
        {
            _gameEventBus = gameEventBus;
            _trackObjectStorage = trackObjectStorage;
        }

        private void Awake()
        {
            // _gameEventBus.SubscribeTo((ref SelectTrackObjectEvent data) => _inputField.text = data.Track.trackObject.TimeDuraction.ToString());
            _gameEventBus.SubscribeTo<SelectObjectEvent>(((ref SelectObjectEvent data) => _inputField.text = data.Tracks[^1].trackObject.TimeDuractionInTicks.ToString()));
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => _inputField.text = "");

            
            _inputField.onEndEdit.AddListener(text =>
            {
                _trackObjectStorage.selectedObject.trackObject.ChangeDurationInTicks(float.Parse(text));
            });
        }
    }
}
