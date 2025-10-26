using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class CompositionOffsetPanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField xOffset;
        [SerializeField] private TMP_InputField yOffset;
        
        private TrackObjectStorage _trackObjectStorage;
private DiContainer _container;
        private GameEventBus _gameEventBus;
        
        [Inject]
        private void Construct(GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage, DiContainer container)
        {
            _gameEventBus = gameEventBus;
            _trackObjectStorage = trackObjectStorage;
            _container = container;
        }
        
        private void Awake()
        {
            _gameEventBus.SubscribeTo(((ref SelectObjectEvent data) =>
            {
                xOffset.onEndEdit.RemoveAllListeners();
                yOffset.onEndEdit.RemoveAllListeners();
                
                if (_trackObjectStorage.GetTrackObjectData(data.Tracks[^1].trackObject) is TrackObjectGroup
                    trackObjectGroup)
                {
                    if (trackObjectGroup.sceneObject.TryGetComponent(out CompositionOffset compositionOffset))
                    {
                        Vector2 offset = compositionOffset.Setup(xOffset, yOffset, trackObjectGroup);
                        xOffset.text = offset.x.ToString();
                        yOffset.text = offset.y.ToString();
                    }
                    else
                    {
                        CompositionOffset offset = trackObjectGroup.sceneObject.AddComponent<CompositionOffset>();
                        _container.Inject(offset);
                        offset.Setup(xOffset, yOffset, trackObjectGroup);
                    }
                }
            }));
        }
    }
}
