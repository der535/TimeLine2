using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.Grid;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.Keyframe.AnimationDatas.BoxCollider.Offset;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.Data;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Bezier_curve.Bezier.View;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine.KeyframeSelect;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class BezierController : MonoBehaviour
    {
        [SerializeField] private BezierPoint bizerPrefab;
        

        [FormerlySerializedAs("timeLineKeyframeScroll")]
        [FormerlySerializedAs("_timeLineKeyframeScroll")]
        [SerializeField]
        private TimeLineKeyframeZoom timeLineKeyframeZoom;

        [SerializeField] private SelectFieldLineController selectFieldLineController;
        [SerializeField] private SelectObjectController selectObjectController;

        private List<List<BezierPoint>> groupPoints = new();
        private bool _active;

        private DiContainer _container;
        private GameEventBus _gameEventBus;
        private M_KeyframeSelectedStorage _keyframeSelectedStorage;
        private ActiveBezierPointsData _activeBezierPoints;
        private TreeViewUI _treeViewUI;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private KeyframeReferences _keyframeReferences;
        private BezierLineDrawer _lineDrawer;
        private VerticalBezierScroll _verticalScroll;
        private VerticalBezierZoom _verticalZoom;

        [Inject]
        void Construct(DiContainer container, GameEventBus gameEventBus, TimeLineConverter timeLineConverter,
            TimeLineSettings timeLineSettings, ActionMap actionMap, M_KeyframeSelectedStorage keyframeSelectedStorage,
            BezierVerticalPosition bezierVerticalPosition, ActiveBezierPointsData activeBezierPointsData,
            TreeViewUI treeViewUI, KeyframeTrackStorage keyframeTrackStorag, KeyframeReferences keyframeReferences,
            BezierLineDrawer lineDrawer, VerticalBezierScroll verticalScroll, VerticalBezierZoom verticalZoom)
        {
            _container = container;
            _gameEventBus = gameEventBus;
            _keyframeSelectedStorage = keyframeSelectedStorage;
            _activeBezierPoints = activeBezierPointsData;
            _treeViewUI = treeViewUI;
            _keyframeTrackStorage = keyframeTrackStorag;
            _keyframeReferences = keyframeReferences;
            _lineDrawer = lineDrawer;
            _verticalScroll = verticalScroll;
            _verticalZoom = verticalZoom;
        }

        private void Start()
        {
            _lineDrawer?.ClearLines();

            _gameEventBus.SubscribeTo((ref AddKeyframeEvent _) => Build(), -1);
            _gameEventBus.SubscribeTo((ref RemoveKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref SelectObjectEvent _) => Build());
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent _) => Build());
            _gameEventBus.SubscribeTo((ref SelectFieldLineEvent _) => Build(), -1);
            _gameEventBus.SubscribeTo((ref DeselectFieldLineEvent _) => Build());
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.KeyframeZoomEvent _) => UpdatePositions());
            _gameEventBus.SubscribeTo((ref ScrollTimeLineKeyframeEvent _) => UpdatePositions());
            _gameEventBus.SubscribeTo((ref ScrollBezier _) => UpdatePositions());
            _gameEventBus.SubscribeTo((ref ZoomBezier _) => UpdatePositions());
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent _) => Clear());
            _gameEventBus.SubscribeTo((ref KeyframeTypeChangeEvent data) =>
            {
                ActiveKeyframes(data.ActiveType == M_KeyframeType.Bezier);
            });
        }

        /// <summary>
        /// Сортируем точки в группе
        /// </summary>
        internal void SortPoints()
        {
            foreach (var group in groupPoints)
            {
                group.Sort((x, y) =>
                    x.BezierDragPoint._keyframe.Ticks.CompareTo(
                        y.BezierDragPoint._keyframe.Ticks));
            }
        }

        /// <summary>
        /// Включает отображение точек кривых если поменялся тип отображения ключевых кадров
        /// </summary>
        /// <param name="active"></param>
        private void ActiveKeyframes(bool active)
        {
            if (selectObjectController.SelectObjects.Count <= 0) return; //Скип если ничего не выделено ????

            _active = active;
            if (_active == false)
            {
                _lineDrawer.ClearLines();
            }

            _lineDrawer?.SetActive(active);

            Build();
            foreach (var group in groupPoints)
            {
                foreach (var point in group)
                {
                    point.gameObject.SetActive(active);
                }
            }

            foreach (var keyframe in _keyframeSelectedStorage.Keyframes)
            {
                BezierPoint point = _activeBezierPoints.Value.Find(x => x.BezierDragPoint._original == keyframe);
                point.Select(true);
                point.BezierSelectPoint.SelectNoEvent();
            }
        }


        // ВАЖНО: Этот метод возвращает позицию ТОЛЬКО на основе value и pan.
        // Скролл НЕ добавляется здесь, чтобы избежать двойного учета!


        private void AddList(ref List<TreeNode> treeNodes, TreeNode node)
        {
            treeNodes.Add(node);

            foreach (var child in node.Children)
            {
                if (child.Children.Count > 0)
                {
                    AddList(ref treeNodes, child);
                }
                else
                {
                    treeNodes.AddRange(node.Children);
                }
            }
        }

        private void Build()
        {
            // selectPointsController.Deselect();

            if (!_active || !gameObject.activeInHierarchy)
            {
                return;
            }

            foreach (var group in groupPoints)
            {
                // Освобождаем старые объекты
                foreach (var point in group)
                {
                    Destroy(point.gameObject);
                }
            }

            groupPoints.Clear();

            _lineDrawer.ClearLines();
            _lineDrawer.ClearBeziers();

            List<TreeNode> activeNodes = new List<TreeNode>();

            _activeBezierPoints.Value.Clear();

            foreach (var tree in _treeViewUI.AnimationLineController.Lines)
            {
                if (tree?.LogicalNode == null) continue;

                Track track = _keyframeTrackStorage.GetTrack(tree.LogicalNode);
                if (track == null) continue;
                List<Keyframe.Keyframe> data = ConvertAverageValues(track);

                if (!activeNodes.Contains(tree.LogicalNode))
                {
                    if (selectFieldLineController.CheckActive(tree.LogicalNode) == false)
                    {
                        continue;
                    }

                    AddList(ref activeNodes, tree.LogicalNode);
                }


                if (data == null) continue;

                List<BezierPoint> points = new List<BezierPoint>();

                for (var index = 0; index < data.Count; index++)
                {
                    var keyframeData = data[index];
                    if (keyframeData.GetData().GetValue() is not float value) continue;

                    BezierPoint point = _container.InstantiatePrefab(bizerPrefab, _keyframeReferences.rootPoints)
                        .GetComponent<BezierPoint>();

                    if (point == null)
                    {
                        continue;
                    }

                    Keyframe.Keyframe prevKey = index - 1 >= 0 ? data[index - 1] : null;
                    Keyframe.Keyframe nextKey = index + 1 < data.Count ? data[index + 1] : null;

                    point.Setup(keyframeData, track.SortKeyframes, prevKey, nextKey, timeLineKeyframeZoom.Zoom,
                        _verticalZoom.Zoom, track.Keyframes[index]);

                    // Конвертируем тики в позицию по X
                    float positionX =
                        TimeLineConverter.Instance.TicksToPositionX(keyframeData.Ticks, timeLineKeyframeZoom.Zoom) +
                        _keyframeReferences.rootObjects.offsetMin.x;

                    // Позиция по Y: сначала получаем "чистую" позицию от value и пана, потом ДОБАВЛЯЕМ скролл
                    float scrollFactor = _verticalZoom.Zoom;
                    float basePositionY = value * scrollFactor; // ← "чистая" позиция
                    float scrolledPositionY =
                        basePositionY + _verticalScroll.VerticalScroll * scrollFactor; // ← добавляем скролл ОДИН РАЗ

                    point.RectTransform.anchoredPosition = new Vector2(positionX, scrolledPositionY);

                    points.Add(point);
                }

                groupPoints.Add(points);
                _lineDrawer.AddPoints(points, track.AnimationColor);
                _activeBezierPoints.Value.AddRange(points);
            }

            _lineDrawer?.UpdateBezierCurve();
        }

        public void DeselectAll()
        {
            _activeBezierPoints.DeselectAll();
        }

        private List<Keyframe.Keyframe> ConvertAverageValues(Track track)
        {
            List<Keyframe.Keyframe> list = new List<Keyframe.Keyframe>();
            foreach (var keyframe in track.Keyframes)
            {
                list.Add(keyframe.Clone());
            }

            switch (track.Keyframes[0].GetData().GetValue())
            {
                case float value:
                    return track.Keyframes;
                case int value:
                    return track.Keyframes;
                case Color value:
                    foreach (var keyframe in list)
                    {
                        Color color = (Color)keyframe.GetData().GetValue();
                        keyframe.AddData(new XOffsetData((color.a + color.r + color.g + color.b) / 4));
                    }

                    return list;
                default:
                    return track.Keyframes;
            }
        }

        private void Clear()
        {
            foreach (var group in groupPoints)
            {
                foreach (var point in group)
                {
                    if (point != null)
                        Destroy(point.gameObject);
                }
            }

            groupPoints.Clear();

            _lineDrawer.ClearLines();
            _lineDrawer.ClearBeziers();
            // selectPointsController.Deselect();
        }

        public void UpdatePositions()
        {
            if (!_active || !gameObject.activeInHierarchy)
            {
                return;
            }

            if (_treeViewUI == null || _keyframeTrackStorage == null)
            {
                return;
            }

            // Обновляем позиции существующих точек
            foreach (var group in groupPoints)
            {
                for (int index = 0; index < group.Count; index++)
                {
                    var point = group[index];
                    if (point == null || point.BezierDragPoint?._keyframe == null)
                        continue;

                    var keyframeData = point.BezierDragPoint._keyframe;
                    if (keyframeData.GetData().GetValue() is not float value)
                        continue;

                    Keyframe.Keyframe prevKey = index - 1 >= 0
                        ? group[index - 1].BezierDragPoint._keyframe
                        : null;
                    Keyframe.Keyframe nextKey = index + 1 < group.Count
                        ? group[index + 1].BezierDragPoint._keyframe
                        : null;

                    point.UpdatePosition(keyframeData, prevKey, nextKey, timeLineKeyframeZoom.Zoom,
                        _verticalZoom.Zoom);

                    // Обновляем X-позицию
                    float positionX =
                        TimeLineConverter.Instance.TicksToPositionX(keyframeData.Ticks, timeLineKeyframeZoom.Zoom) +
                       _keyframeReferences.rootObjects.offsetMin.x;

                    // Обновляем Y-позицию
                    float scrollFactor = _verticalZoom.Zoom;
                    float basePositionY = value * scrollFactor;
                    float scrolledPositionY = basePositionY + _verticalScroll.VerticalScroll * scrollFactor;

                    point.RectTransform.anchoredPosition = new Vector2(positionX, scrolledPositionY);
                }
            }

            // Перерисовываем кривые
            _lineDrawer?.UpdateBezierCurve();
        }
    }
}