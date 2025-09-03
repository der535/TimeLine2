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
        [Space] [SerializeField] private TreeViewUI treeViewUI;
        [SerializeField] private KeyframeTrackStorage keyframeTrackStorage;

        private List<KeyframeObjectData> keyframes = new();
        private Main _main;
        private DiContainer _container;
        private GameEventBus _gameEventBus;

        private KeyframeSelect _keyframeObjectSelect;
        private Keyframe.Keyframe _keyframeSelect;
        private TimeLineConverter _timeLineConverter;
        public KeyframeObjectData SelectedKeyframe { get; private set; }

        [Inject]
        private void Construct(GameEventBus gameEventBus, Main main, DiContainer container,
            TimeLineConverter timeLineConverter)
        {
            _container = container;
            _gameEventBus = gameEventBus;
            _main = main;
            _timeLineConverter = timeLineConverter;
        }

        void Awake()
        {
            _gameEventBus.SubscribeTo((ref AddKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref RemoveKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref SelectObjectEvent _) => Build());
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
                    float positionX = _timeLineConverter.TicksToPositionX(keyframe.ticks);
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
            _gameEventBus.UnsubscribeFrom<AddKeyframeEvent>((ref AddKeyframeEvent _) => Build());
            _gameEventBus.UnsubscribeFrom<RemoveKeyframeEvent>((ref RemoveKeyframeEvent _) => Build());
            _gameEventBus.UnsubscribeFrom<SelectObjectEvent>((ref SelectObjectEvent _) => Build());
            _gameEventBus.UnsubscribeFrom<SelectKeyframeEvent>(SelectKeyframe);
        }
    }
}