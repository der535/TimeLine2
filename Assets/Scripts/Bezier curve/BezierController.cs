using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class BezierController : MonoBehaviour
    {
        [SerializeField] private TreeViewUI treeViewUI;
        [SerializeField] private BezierPoint bizerPrefab;
        [SerializeField] private KeyframeTrackStorage keyframeTrackStorage;
        [SerializeField] private RectTransform _content;
        [SerializeField] private BezierTest test;

        [SerializeField] private TimeLineKeyframeScroll _timeLineKeyframeScroll;

        private TimeLineConverter _timeLineConverter;
        private List<BezierPoint> keyframes = new();


        private DiContainer _container;

        private GameEventBus _gameEventBus;

        [Inject]
        void Construct(DiContainer container, GameEventBus gameEventBus, TimeLineConverter timeLineConverter)
        {
            _container = container;
            _gameEventBus = gameEventBus;
            _timeLineConverter = timeLineConverter;
        }
        
        private void Start()
        {
            test.Clear();
            _gameEventBus.SubscribeTo((ref AddKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref RemoveKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref SelectObjectEvent _) => Build());
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.PanEvent _) => Build());
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.ScrollTimeLineKeyframeEvent _) => Build());
        }
        
        private void Build()
        {
            foreach (var keyframe in keyframes.Where(keyframe => keyframe))
                Destroy(keyframe.gameObject);
            keyframes = new List<BezierPoint>();
            
            test.Clear();
            foreach (var tree in treeViewUI.NodeObjects)
            {
                Track track = keyframeTrackStorage.GetTrack(tree.LogicalNode);

                if (track == null) continue;
                foreach (var keyframe in track.Keyframes)
                {
                    BezierPoint keyframeObjectData = _container.InstantiatePrefab(bizerPrefab, tree.RootRect)
                        .GetComponent<BezierPoint>();
                    keyframes.Add(keyframeObjectData.GetComponent<BezierPoint>());
                    // KeyframeDrag keyframeDrag = keyframeObjectData.GetComponent<KeyframeDrag>();

                    // Конвертируем тики в позицию на таймлайне
                    if (keyframe.GetData().GetValue() is float value)
                    {
                        float positionX = _timeLineConverter.TicksToPositionX(keyframe.Ticks, _timeLineKeyframeScroll.Pan)+_content.offsetMin.x;
                        keyframeObjectData.RectTransform.anchoredPosition = new Vector2(
                            positionX,
                            value * 70);
                        test.AddPoint(keyframeObjectData);
                    }

                    // keyframeDrag.Setup(keyframe, track.SortKeyframes);
                    // keyframeObjectData.Keyframe = keyframe;
                    // keyframeObjectData.Track = track;
                }
            }
        }
    }
}
