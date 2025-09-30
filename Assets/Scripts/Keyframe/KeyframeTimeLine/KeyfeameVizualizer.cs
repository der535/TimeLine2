using System.Collections.Generic;
using System.Linq;
using EventBus;
using NaughtyAttributes;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class KeyfeameVizualizer : MonoBehaviour
    {
        [SerializeField] private KeyframeObjectData keyFrame;
        [SerializeField] private TimeLineSettings timeLineSettings;
        [Space]
        [SerializeField] private TreeViewUI treeViewUI;
        [SerializeField] private KeyframeTrackStorage keyframeTrackStorage;
        [Space] 
        [SerializeField] private TimeLineKeyframeScroll _timeLineKeyframeScroll;
        [SerializeField] private RectTransform _content;
        [FormerlySerializedAs("keframeScrollView")]
        [FormerlySerializedAs("keframeScrollV")]
        [FormerlySerializedAs("fieldPanel")]
        [Header("Keyframe offset")]
        [SerializeField] private RectTransform keyframeScrollView;
        [FormerlySerializedAs("verticalLayoutGroup")] [SerializeField] private VerticalLayoutGroup keyframeVerticalLayoutGroup;

        private DiContainer _container;
        private List<KeyframeObjectData> _keyframes = new();
        private GameEventBus _gameEventBus;

        private KeyframeSelect _keyframeObjectSelect;
        private Keyframe.Keyframe _keyframeSelect;
        private TimeLineConverter _timeLineConverter;

        private bool _active;
        public KeyframeObjectData SelectedKeyframe { get; private set; }

        [Inject]
        private void Construct(GameEventBus gameEventBus, DiContainer container,
            TimeLineConverter timeLineConverter)
        {
            _container = container;
            _gameEventBus = gameEventBus;
            _timeLineConverter = timeLineConverter;
        }

        void Awake()
        {
            _gameEventBus.SubscribeTo((ref AddKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref RemoveKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref SelectObjectEvent _) => Build());
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.PanEvent _) => Build());
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.ScrollTimeLineKeyframeEvent _) => Build());
            
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => Clear());

            _gameEventBus.SubscribeTo<SelectKeyframeEvent>(SelectKeyframe);
        }

        public void ActiveKeyframes(bool active)
        {
            _active = active;
            Build();
            foreach (var keyframe in _keyframes)
            {
                keyframe.RectTransform.gameObject.SetActive(active);
            }
        }

        private void SelectKeyframe(ref SelectKeyframeEvent selectKeyframeEvent)
        {
            SelectedKeyframe = selectKeyframeEvent.Keyframe;
            foreach (var keyframe in _keyframes)
            {
                keyframe.KeyframeSelect.SelectColor(false);
            }
            selectKeyframeEvent.Keyframe.KeyframeSelect.SelectColor(true);
        }

        [Button]
        private void Build()
        {
            if(!_active) return;
            
            // print("Build");
            
            foreach (var keyframe in _keyframes.Where(keyframe => keyframe))
                Destroy(keyframe.gameObject);
            
            _keyframes = new List<KeyframeObjectData>();
            
            // print(treeViewUI.AnimationLineController.Lines.Count);

            foreach (var tree in treeViewUI.AnimationLineController.Lines)
            {
                Track track = keyframeTrackStorage.GetTrack(tree.LogicalNode);
                
                // print(track);
                
                if (track == null) continue;
                
                // print(track.Keyframes.Count);
                
                foreach (var keyframe in track.Keyframes)
                {
                    KeyframeObjectData keyframeObjectData = _container.InstantiatePrefab(keyFrame, tree.KeyframeLine)
                        .GetComponent<KeyframeObjectData>();
                    
                    _keyframes.Add(keyframeObjectData.GetComponent<KeyframeObjectData>());
                    KeyframeDrag keyframeDrag = keyframeObjectData.GetComponent<KeyframeDrag>();

                    // Конвертируем тики в позицию на таймлайне
                    float xOffset = _content.offsetMin.x - keyframeScrollView.offsetMin.x - keyframeVerticalLayoutGroup.padding.left / 2f;
                    
                    float positionX = _timeLineConverter.TicksToPositionX(keyframe.Ticks, _timeLineKeyframeScroll.Pan) + xOffset;
                    keyframeObjectData.RectTransform.anchoredPosition = new Vector2(
                        positionX,
                        keyframeObjectData.RectTransform.anchoredPosition.y);

                    keyframeDrag.Setup(keyframe, track.SortKeyframes);
                    keyframeObjectData.Keyframe = keyframe;
                    keyframeObjectData.Track = track;
                }
            }
        }

        private void Clear()
        {
            foreach (var keyframe in _keyframes.Where(keyframe => keyframe))
                Destroy(keyframe.gameObject);
        }
        
        private void OnDestroy()
        {
            _gameEventBus.UnsubscribeFrom((ref AddKeyframeEvent _) => Build());
            _gameEventBus.UnsubscribeFrom((ref RemoveKeyframeEvent _) => Build());
            _gameEventBus.UnsubscribeFrom((ref SelectObjectEvent _) => Build());
            _gameEventBus.UnsubscribeFrom<SelectKeyframeEvent>(SelectKeyframe);
        }
    }
}