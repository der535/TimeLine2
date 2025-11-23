using System;
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
        [Space] [SerializeField] private TreeViewUI treeViewUI;
        [SerializeField] private KeyframeTrackStorage keyframeTrackStorage;
        [Space] [SerializeField] private TimeLineKeyframeScroll _timeLineKeyframeScroll;
        [SerializeField] private RectTransform _content;

        [FormerlySerializedAs("keframeScrollView")]
        [FormerlySerializedAs("keframeScrollV")]
        [FormerlySerializedAs("fieldPanel")]
        [Header("Keyframe offset")]
        [SerializeField]
        private RectTransform keyframeScrollView;

        [FormerlySerializedAs("verticalLayoutGroup")] [SerializeField]
        private VerticalLayoutGroup keyframeVerticalLayoutGroup;

        private DiContainer _container;
        private List<KeyframeObjectData> _keyframes = new();
        private GameEventBus _gameEventBus;

        private KeyframeSelect _keyframeObjectSelect;
        private Keyframe.Keyframe _keyframeSelect;
        private TimeLineConverter _timeLineConverter;
        private ActionMap _animationMap;

        private bool _active;
        public List<KeyframeObjectData> SelectedKeyframe { get; private set; }

        [Inject]
        private void Construct(GameEventBus gameEventBus, DiContainer container,
            TimeLineConverter timeLineConverter, ActionMap actionMap)
        {
            _container = container;
            _gameEventBus = gameEventBus;
            _timeLineConverter = timeLineConverter;
            _animationMap = actionMap;
        }

        public double GetMinTimeSelectedKeyframe(List<KeyframeObjectData> keyframe)
        {
            if (!keyframe.Any()) 
                throw new InvalidOperationException("No keyframes selected.");
            return keyframe.Min(k => (double)k.Keyframe.Ticks);
        }

        public double GetMaxTimeSelectedKeyframe()
        {
            if (!SelectedKeyframe.Any()) 
                throw new InvalidOperationException("No keyframes selected.");
            return SelectedKeyframe.Max(k => (double)k.Keyframe.Ticks);
        }

        void Awake()
        {
            SelectedKeyframe = new List<KeyframeObjectData>();
            // _animationMap.Editor.I.started += context => InverKeyframes();
            
            _gameEventBus.SubscribeTo((ref AddKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref RemoveKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref SelectObjectEvent _) => Build());
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.PanEvent _) => PoseKeyframes());
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.ScrollTimeLineKeyframeEvent _) => PoseKeyframes());

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

        public List<KeyframeObjectData> GetKeyframesData(List<BezierPoint> bezierPoints)
        {
            return _keyframes.Where(k => bezierPoints.Any(b => k.Keyframe == b.BezierDragPoint._keyframe)).ToList();
        }

        private void SelectKeyframe(ref SelectKeyframeEvent selectKeyframeEvent)
        {
            if (_animationMap.Editor.LeftShift.IsPressed())
            {
                if (SelectedKeyframe.Contains(selectKeyframeEvent.Keyframe))
                {
                    SelectedKeyframe.Remove(selectKeyframeEvent.Keyframe);
                }
                else
                {
                    SelectedKeyframe.Add(selectKeyframeEvent.Keyframe);
                }
            }
            else
            {
                if (!SelectedKeyframe.Contains(selectKeyframeEvent.Keyframe))
                {
                    SelectedKeyframe.Clear();
                    SelectedKeyframe.Add(selectKeyframeEvent.Keyframe);
                }
            }
            
            print(SelectedKeyframe.Count);

            foreach (var keyframe in _keyframes)
            {
                keyframe.KeyframeSelect.SelectColor(false);
            }

            foreach (var sData in SelectedKeyframe)
            {
                sData.KeyframeSelect.SelectColor(true);
            }
        }

        public void MultipleDrag(double offset, KeyframeDrag drag)
        {
            if(offset == 0) return;
            
            // print(offset);
            
            foreach (var keyframe in SelectedKeyframe)
            {
                if(drag == keyframe.KeyframeDrag) continue;
                
                float xOffset = _content.offsetMin.x - keyframeScrollView.offsetMin.x -
                                keyframeVerticalLayoutGroup.padding.left / 2f;

                float positionX =
                    _timeLineConverter.TicksToPositionX(keyframe.Keyframe.Ticks+=offset, _timeLineKeyframeScroll.Pan) +
                    xOffset;
                keyframe.RectTransform.anchoredPosition = new Vector2(
                    positionX,
                    keyframe.RectTransform.anchoredPosition.y);
            }
        }

        public void DeselectAllKeyframes()
        {
            foreach (var keyframe in SelectedKeyframe)
            {
                if(keyframe != null) keyframe.KeyframeSelect.SelectColor(false);
            }
            SelectedKeyframe.Clear();
        }

        internal List<(Track, Keyframe.Keyframe)> GetAllKeyframes()
        {
            var result = new List<(Track, Keyframe.Keyframe)>();
            
            _keyframes = new List<KeyframeObjectData>();

            foreach (var tree in treeViewUI.AnimationLineController.Lines)
            {
                Track track = keyframeTrackStorage.GetTrack(tree.LogicalNode);

                if (track == null) continue;

                foreach (var keyframe in track.Keyframes)
                {
                    result.Add((track, keyframe));
                }
            }
            return result;
        }

        [Button]
        private void Build()
        {
            if (!_active) return;

            foreach (var keyframe in _keyframes.Where(keyframe => keyframe))
                Destroy(keyframe.gameObject);

            _keyframes = new List<KeyframeObjectData>();

            foreach (var tree in treeViewUI.AnimationLineController.Lines)
            {
                Track track = keyframeTrackStorage.GetTrack(tree.LogicalNode);

                if (track == null) continue;

                foreach (var keyframe in track.Keyframes)
                {
                    KeyframeObjectData keyframeObjectData = _container.InstantiatePrefab(keyFrame, tree.KeyframeLine)
                        .GetComponent<KeyframeObjectData>();

                    KeyframeDrag keyframeDrag = keyframeObjectData.GetComponent<KeyframeDrag>();

                    keyframeDrag.Setup(keyframe, track.SortKeyframes);
                    keyframeObjectData.Keyframe = keyframe;
                    keyframeObjectData.Track = track;
                    
                    _keyframes.Add(keyframeObjectData);
                }
            }

            PoseKeyframes();
        }

        private void InverKeyframes()
        {
            var min = GetMinTimeSelectedKeyframe(SelectedKeyframe);
            var max = GetMaxTimeSelectedKeyframe();
            // x' = min + (max - x)
            foreach (var keyframe in _keyframes)
            {
                keyframe.Keyframe.Ticks = min + (max - keyframe.Keyframe.Ticks);
            }

            foreach (var tree in treeViewUI.AnimationLineController.Lines)
            {
                Track track = keyframeTrackStorage.GetTrack(tree.LogicalNode);
                if(track == null) continue;
                track.SortKeyframes();
            }

            PoseKeyframes();
        }

        private void PoseKeyframes()
        {
            foreach (var keyframe in _keyframes)
            {
                // Конвертируем тики в позицию на таймлайне
                float xOffset = _content.offsetMin.x - keyframeScrollView.offsetMin.x -
                                keyframeVerticalLayoutGroup.padding.left / 2f;

                float positionX =
                    _timeLineConverter.TicksToPositionX(keyframe.Keyframe.Ticks, _timeLineKeyframeScroll.Pan) +
                    xOffset;
                keyframe.RectTransform.anchoredPosition = new Vector2(
                    positionX,
                    keyframe.RectTransform.anchoredPosition.y);
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