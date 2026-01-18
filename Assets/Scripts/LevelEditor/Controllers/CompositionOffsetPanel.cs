using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class CompositionOffsetPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [Space]
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
            panel.SetActive(false);
            
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent eventData) =>
            {
                panel.SetActive(false);
            });
            
            _gameEventBus.SubscribeTo(((ref SelectObjectEvent data) =>
            {
                xOffset.onEndEdit.RemoveAllListeners();
                xOffset.onValueChanged.RemoveAllListeners();
                yOffset.onEndEdit.RemoveAllListeners();
                yOffset.onValueChanged.RemoveAllListeners();
                
                if (_trackObjectStorage.GetTrackObjectData(data.Tracks[^1].trackObject) is TrackObjectGroup
                    trackObjectGroup)
                {
                    panel.SetActive(true);
                    
                    if (trackObjectGroup.sceneObject.TryGetComponent(out CompositionOffset compositionOffset))
                    {
                        // print(compositionOffset.XOffset.Value);
                        xOffset.text = compositionOffset.XOffset.Value.ToString();
                        yOffset.text = compositionOffset.YOffset.Value.ToString();
                        compositionOffset.Setup(xOffset, yOffset, trackObjectGroup);
                    }
                    else
                    {
                        CompositionOffset offset = trackObjectGroup.sceneObject.AddComponent<CompositionOffset>();
                        _container.Inject(offset);
                        offset.Setup(xOffset, yOffset, trackObjectGroup);
                    }
                }
            }));
            
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent eventData) =>
            {
                xOffset.onEndEdit.RemoveAllListeners();
                xOffset.onValueChanged.RemoveAllListeners();
                yOffset.onEndEdit.RemoveAllListeners();
                yOffset.onValueChanged.RemoveAllListeners();
                
                if (_trackObjectStorage.GetTrackObjectData(eventData.SelectedObjects[^1].trackObject) is TrackObjectGroup
                    trackObjectGroup)
                {
                    panel.SetActive(true);
                    
                    if (trackObjectGroup.sceneObject.TryGetComponent(out CompositionOffset compositionOffset))
                    {
                        print(compositionOffset.XOffset.Value);
                        xOffset.text = compositionOffset.XOffset.Value.ToString();
                        yOffset.text = compositionOffset.YOffset.Value.ToString();
                        compositionOffset.Setup(xOffset, yOffset, trackObjectGroup);
                    }
                    else
                    {
                        CompositionOffset offset = trackObjectGroup.sceneObject.AddComponent<CompositionOffset>();
                        _container.Inject(offset);
                        offset.Setup(xOffset, yOffset, trackObjectGroup);
                    }
                }
            });
        }
    }
}
