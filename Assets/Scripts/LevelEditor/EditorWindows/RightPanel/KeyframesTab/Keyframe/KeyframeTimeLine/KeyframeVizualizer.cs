using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using NaughtyAttributes;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine.KeyframeSelect;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class KeyframeVizualizer : MonoBehaviour
    {
        [SerializeField] private KeyframeObjectData keyFrame;
        [SerializeField] private TimeLineSettings timeLineSettings;
        [Space] [SerializeField] private TreeViewUI treeViewUI;
        [SerializeField] private KeyframeTrackStorage keyframeTrackStorage;

        [FormerlySerializedAs("_timeLineKeyframeScroll")] [Space] [SerializeField]
        private TimeLineKeyframeZoom timeLineKeyframeZoom;

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
        private M_KeyframeSelectedStorage _selectedKeyframesStorage;

        private bool _active;

        [Inject]
        private void Construct(GameEventBus gameEventBus, DiContainer container,
            TimeLineConverter timeLineConverter, ActionMap actionMap, M_KeyframeSelectedStorage storage,
            KeyframeSelectController keyframeTrackStorage)
        {
            _container = container;
            _gameEventBus = gameEventBus;
            _timeLineConverter = timeLineConverter;
            _selectedKeyframesStorage = storage;
        }

        public double GetMinTimeSelectedKeyframe(List<Keyframe.Keyframe> keyframe)
        {
            if (!keyframe.Any())
                throw new InvalidOperationException("No keyframes selected.");
            return keyframe.Min(k => (double)k.Ticks);
        }

        public double GetMaxTimeSelectedKeyframe()
        {
            if (!_selectedKeyframesStorage.Keyframes.Any())
                throw new InvalidOperationException("No keyframes selected.");
            return _selectedKeyframesStorage.Keyframes.Max(k => k.Ticks);
        }

        public KeyframeObjectData GetKeyframeObjectData(Keyframe.Keyframe key)
        {
            return _keyframes.FirstOrDefault(k => k.Keyframe == key);
        }

        public List<KeyframeObjectData> GetAllKeyframesObjectData()
        {
            return _keyframes;
        }

        public void DisableAll()
        {
            foreach (var keyframe in _keyframes)
            {
                keyframe.KeyframeSelect.SelectColor(false);
            }
        }

        void Awake()
        {
            // _animationMap.Editor.I.started += context => InverKeyframes();

            _gameEventBus.SubscribeTo((ref AddKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref RemoveKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref SelectObjectEvent _) => Build());
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent _) =>
            {

                Build();
            });
            // _gameEventBus.SubscribeTo((ref DeselectObjectEvent _) => Build());
            _gameEventBus.SubscribeTo((ref KeyframeZoomEvent _) => PoseKeyframes());
            _gameEventBus.SubscribeTo((ref ScrollTimeLineKeyframeEvent _) =>
                PoseKeyframes());

            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) =>
            {
                foreach (var keyframe in _keyframes.Where(keyframe => keyframe))
                    Destroy(keyframe.gameObject);
                _keyframes = new List<KeyframeObjectData>();
                Clear();
            });
            _gameEventBus.SubscribeTo((ref KeyframeTypeChangeEvent data) =>
            {
                ActiveKeyframes(data.ActiveType == M_KeyframeType.Keyframe);
            });
        }

        public void ActiveKeyframes(bool active)
        {
            _active = active;
            foreach (var keyframe in _keyframes.Where(keyframe => keyframe))
                Destroy(keyframe.gameObject);
            _keyframes = new List<KeyframeObjectData>();
            Build();
            foreach (var keyframe in _keyframes)
            {
                keyframe.RectTransform.gameObject.SetActive(active);
                if (_selectedKeyframesStorage.Keyframes.Contains(keyframe.KeyframeDrag._keyframe))
                {
                    Debug.Log(keyframe.Keyframe.Ticks, keyframe.KeyframeDrag.gameObject);

                    keyframe.KeyframeSelect.SelectColor(true);
                }
            }
            
        }


        public void MultipleDrag(double offset, KeyframeDrag drag)
        {
            if (offset == 0) return;

            // print(offset);

            foreach (var keyframe in _selectedKeyframesStorage.Keyframes)
            {
                KeyframeObjectData keyframeObjectData = GetKeyframeObjectData(keyframe);
                if (drag == keyframeObjectData.KeyframeDrag) continue;

                float xOffset = _content.offsetMin.x - keyframeScrollView.offsetMin.x -
                                keyframeVerticalLayoutGroup.padding.left / 2f;

                float positionX =
                    _timeLineConverter.TicksToPositionX(keyframeObjectData.Keyframe.Ticks += offset,
                        timeLineKeyframeZoom.Zoom) +
                    xOffset;
                keyframeObjectData.RectTransform.anchoredPosition = new Vector2(
                    positionX,
                    keyframeObjectData.RectTransform.anchoredPosition.y);
            }
        }

        public void DeselectAllKeyframes()
        {
            _gameEventBus.Raise(new DeselectAllKeyframeEvent());
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
            foreach (var keyframe in _keyframes.Where(keyframe => keyframe))
                Destroy(keyframe.gameObject);
            _keyframes = new List<KeyframeObjectData>();
            
            if (!_active) return;
            
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
            var min = GetMinTimeSelectedKeyframe(_selectedKeyframesStorage.Keyframes);
            var max = GetMaxTimeSelectedKeyframe();
            // x' = min + (max - x)
            foreach (var keyframe in _keyframes)
            {
                keyframe.Keyframe.Ticks = min + (max - keyframe.Keyframe.Ticks);
            }

            foreach (var tree in treeViewUI.AnimationLineController.Lines)
            {
                Track track = keyframeTrackStorage.GetTrack(tree.LogicalNode);
                if (track == null) continue;
                track.SortKeyframes();
            }

            PoseKeyframes();
        }

        private void PoseKeyframes()
        {
            if (!_active) return;

            foreach (var keyframe in _keyframes)
            {
                // Конвертируем тики в позицию на таймлайне
                float xOffset = _content.offsetMin.x - keyframeScrollView.offsetMin.x -
                                keyframeVerticalLayoutGroup.padding.left / 2f;

                float positionX =
                    _timeLineConverter.TicksToPositionX(keyframe.Keyframe.Ticks, timeLineKeyframeZoom.Zoom) +
                    xOffset;
                keyframe.RectTransform.anchoredPosition = new Vector2(
                    positionX,
                    keyframe.RectTransform.anchoredPosition.y);
            }
        }

        private void Clear()
        {
            foreach (var keyframe in _keyframes.Where(keyframe => keyframe).ToList())
            {
                Destroy(keyframe.gameObject);
                _keyframes.Remove(keyframe);
            }
        }
    }
}