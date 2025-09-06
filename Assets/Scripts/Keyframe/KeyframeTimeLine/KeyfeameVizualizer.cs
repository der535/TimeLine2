using System.Collections.Generic;
using System.Linq;
using EventBus;
using NaughtyAttributes;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.TimeLine;
using UnityEngine;
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

        private List<KeyframeObjectData> keyframes = new();
        private DiContainer _container;
        private GameEventBus _gameEventBus;

        private KeyframeSelect _keyframeObjectSelect;
        private Keyframe.Keyframe _keyframeSelect;
        private TimeLineConverter _timeLineConverter;
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
            _gameEventBus.SubscribeTo<SelectKeyframeEvent>(SelectKeyframe);
        }

        private void SelectKeyframe(ref SelectKeyframeEvent selectKeyframeEvent)
        {
            SelectedKeyframe = selectKeyframeEvent.Keyframe;
            foreach (var keyframe in keyframes)
            {
                keyframe.KeyframeSelect.SelectColor(false);
            }
            selectKeyframeEvent.Keyframe.KeyframeSelect.SelectColor(true);
        }

        [Button]
        private void Build()
        {
            foreach (var keyframe in keyframes.Where(keyframe => keyframe))
                Destroy(keyframe.gameObject);
            keyframes = new List<KeyframeObjectData>();

            foreach (var tree in treeViewUI.NodeObjects)
            {
                Track track = keyframeTrackStorage.GetTrack(tree.LogicalNode);

                if (track == null) continue;
                foreach (var keyframe in track.Keyframes)
                {
                    KeyframeObjectData keyframeObjectData = _container.InstantiatePrefab(keyFrame, tree.RootRect)
                        .GetComponent<KeyframeObjectData>();
                    keyframes.Add(keyframeObjectData.GetComponent<KeyframeObjectData>());
                    KeyframeDrag keyframeDrag = keyframeObjectData.GetComponent<KeyframeDrag>();

                    // Конвертируем тики в позицию на таймлайне
                    float positionX = _timeLineConverter.TicksToPositionX(keyframe.ticks, _timeLineKeyframeScroll.Pan)+_content.offsetMin.x;
                    keyframeObjectData.RectTransform.anchoredPosition = new Vector2(
                        positionX,
                        keyframeObjectData.RectTransform.anchoredPosition.y);

                    keyframeDrag.Setup(keyframe, track.SortKeyframes);
                    keyframeObjectData.Keyframe = keyframe;
                    keyframeObjectData.Track = track;
                }
            }
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