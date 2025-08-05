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
            _gameEventBus.SubscribeTo((ref SelectTrackObjectEvent data) => _inputField.text = data.Track.trackObject.TimeDuraction.ToString());
            
            _inputField.onEndEdit.AddListener(text =>
            {
                _trackObjectStorage._selectedObject.trackObject.ChangeDuration(float.Parse(text));
            });
        }
    }
}
