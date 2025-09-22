using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class BezierController : MonoBehaviour
    {
        [SerializeField] private TreeViewUI treeViewUI;
        [SerializeField] private BezierPoint bizerPrefab;
        [SerializeField] private KeyframeTrackStorage keyframeTrackStorage;
        [SerializeField] private RectTransform _content;
        [SerializeField] private RectTransform rootPoints;
        [SerializeField] private BezierLineDrawer bezierLineDrawer;
        [SerializeField] private BezierLineDrawer lineDrawer;
        [SerializeField] private VerticalBezierScroll verticalScroll;
        [SerializeField] private VerticalBezierPan verticalPan;
        [SerializeField] private TimeLineKeyframeScroll _timeLineKeyframeScroll;
        [SerializeField] private Camera camera;
        [SerializeField] private RectTransform centerLine;

        private TimeLineConverter _timeLineConverter;
        private List<BezierPoint> keyframes = new();
        private bool _active;

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
            lineDrawer.Clear();
            _gameEventBus.SubscribeTo((ref AddKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref RemoveKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref SelectObjectEvent _) => Build());
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.PanEvent _) => Build());
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.ScrollTimeLineKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref ScrollBezier _) => Build());
            _gameEventBus.SubscribeTo((ref PanBezier _) => Build());
        }

        public void ActiveKeyframes(bool active)
        {
            _active = active;
            bezierLineDrawer.SetActive(active);
            Build();
            foreach (var keyframe in keyframes)
            {
                keyframe.gameObject.SetActive(active);
            }
        }

        public Vector2 GetCursorPosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_content,
                UnityEngine.Input.mousePosition, camera, out var vector2);
            return vector2;
        }

        private void Update()
        {
            
            
            print(GetCursorPosition().y / (VerticalBezierScroll.ScrollMultiplier + verticalPan.Pan) + verticalScroll.VerticalScroll * (VerticalBezierScroll.ScrollMultiplier + verticalPan.Pan));
        }

        private void Build()
        {
            if (!_active || !gameObject.activeInHierarchy) return;

            // Освобождаем старые объекты
            foreach (var keyframe in keyframes)
            {
                if (keyframe != null)
                    Destroy(keyframe.gameObject);
            }

            keyframes.Clear();

            bezierLineDrawer?.Clear();

            if (treeViewUI == null || keyframeTrackStorage == null || _timeLineConverter == null)
            {
                Debug.LogWarning("Some dependencies are not assigned in BezierController.");
                return;
            }

            foreach (var tree in treeViewUI.NodeObjects)
            {
                if (tree?.LogicalNode == null) continue;

                Track track = keyframeTrackStorage.GetTrack(tree.LogicalNode);
                if (track == null) continue;

                foreach (var keyframeData in track.Keyframes)
                {
                    if (keyframeData.GetData().GetValue() is not float value) continue;

                    BezierPoint point = _container.InstantiatePrefab(bizerPrefab, rootPoints)
                        .GetComponent<BezierPoint>();

                    if (point == null)
                    {
                        Debug.LogError("Failed to instantiate BezierPoint prefab.");
                        continue;
                    }

                    keyframes.Add(point);

                    // Конвертируем тики в позицию по X
                    float positionX =
                        _timeLineConverter.TicksToPositionX(keyframeData.Ticks, _timeLineKeyframeScroll.Pan) +
                        _content.offsetMin.x;

                    // Позиция по Y с учётом скролла и масштаба
                    float scrollFactor = VerticalBezierScroll.ScrollMultiplier + verticalPan.Pan;
                    float positionY = value * scrollFactor + verticalScroll.VerticalScroll * scrollFactor;

                    point.RectTransform.anchoredPosition = new Vector2(positionX, positionY);

                    bezierLineDrawer?.AddPoint(point);
                }
            }
        }
        
        // --- Делаем как тут
        // private void Start()
        // {
        //     _gameEventBus.SubscribeTo((ref ScrollTimeLineEvent scrollEvent) =>
        //     {
        //         _mainObjects.ContentRectTransform.offsetMin += new Vector2(scrollEvent.ScrollOffset, 0); //Left
        //         _mainObjects.ContentRectTransform.offsetMax += new Vector2(scrollEvent.ScrollOffset, 0); //Right
        //         _mainObjects.NotifyContentRectChanged();
        //     }, 1);
        //     _gameEventBus.SubscribeTo((ref OldPanEvent oldPanEvent) => _oldPan = oldPanEvent.OldPanOffset,1);
        //     _gameEventBus.SubscribeTo((ref PanEvent _) =>
        //     {
        //         float curPos = (float)_timeLineConverter.GetCursorBeatPosition(_oldPan, 0);
        //         _timeLineRenderer.SetPosition(-(_timeLineConverter.GetAnchorPositionFromBeatPosition(curPos) -
        //                                         _timeLineConverter.CursorPosition().x));
        //     },1);
        // }
    }
}
