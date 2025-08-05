using EventBus;
using NaughtyAttributes;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class KeyfeameVizualizer : MonoBehaviour
    {
        [SerializeField] private GameObject keyFrame;
        [SerializeField] private TimeLineSettings timeLineSettings;
        
        private Main _main;

        [Space] [SerializeField] private TreeViewUI treeViewUI;

        // [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private KeyframeTrackStorage keyframeTrackStorage;

        private GameEventBus _gameEventBus;
        
        [Inject]
        private void Construct(GameEventBus gameEventBus, Main main)
        {
            _gameEventBus = gameEventBus;
            _main = main;
        }

        void Awake()
        {
            _gameEventBus.SubscribeTo((ref AddKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref SelectTrackObjectEvent _) => Build());
        }

        [Button]
        private void Build()
        {
            foreach (var tree in treeViewUI.NodeObjects)
            {
                Track track = keyframeTrackStorage.GetTrack(tree.LogicalNode);

                if (track == null) continue;
                foreach (var keyframe in track.keyframes)
                {
                    RectTransform keyframeRect = Instantiate(keyFrame, tree.RootRect).GetComponent<RectTransform>();
                    keyframeRect.anchoredPosition = new Vector2(keyframe.time * (timeLineSettings.DistanceBetweenBeatLines * (_main.MusicDataSo.bpm / 60)), keyframeRect.anchoredPosition.y);
                }
            }
        }
    }
}