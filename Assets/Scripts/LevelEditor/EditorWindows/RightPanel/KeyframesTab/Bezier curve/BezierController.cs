using System;
using System.Collections.Generic;
using System.IO;
using EventBus;
using TimeLine;
using TimeLine.EventBus.Events.Grid;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.Keyframe.AnimationDatas.BoxCollider.Offset;
using TimeLine.Keyframe.KeyframeTimeLine;
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

        [FormerlySerializedAs("_content")] [SerializeField]
        private RectTransform content;

        [SerializeField] private RectTransform rootPoints;
        [SerializeField] private BezierLineDrawer bezierLineDrawer;
        [SerializeField] private BezierLineDrawer lineDrawer; // Возможно дублирует bezierLineDrawer? Проверьте.
        [SerializeField] private VerticalBezierScroll verticalScroll;

        [FormerlySerializedAs("verticalPan")] [SerializeField]
        private VerticalBezierZoom verticalZoom;

        [FormerlySerializedAs("_timeLineKeyframeScroll")] [SerializeField]
        private TimeLineKeyframeScroll timeLineKeyframeScroll;

        [FormerlySerializedAs("camera")] [SerializeField]
        private Camera editCameraUI;

        [SerializeField] private RectTransform centerLine;
        [SerializeField] private RectTransform root;
        [SerializeField] private float spacing = 30;

        [FormerlySerializedAs("ScrollTimeLineKeyframe")] [SerializeField]
        private ScrollTimeLineKeyframe scrollTimeLineKeyframe;

        [FormerlySerializedAs("verticalBezierPan")] [SerializeField]
        private VerticalBezierZoom verticalBezierZoom;

        [Header("Logging")] [SerializeField]
        private string logFilePath = ""; // Укажите путь вручную, например: "C:/Temp/BezierController.log"

        [SerializeField] private SelectFieldLineController selectFieldLineController;
        [SerializeField] private BezierSelectPointsController selectPointsController;
        [SerializeField] private SelectObjectController selectObjectController;

        [SerializeField] private RectTransform leftPanelAnimations;

        private TimeLineSettings _timeLineSettings;
        private TimeLineConverter _timeLineConverter;
        private List<BezierPointsGroup> groups = new();
        private bool _active;

        private DiContainer _container;
        private GameEventBus _gameEventBus;
        private ActionMap _actionMap;

        private List<BezierPoint> activePoints = new();


        private string LogFile => string.IsNullOrEmpty(logFilePath)
            ? Path.Combine(Application.persistentDataPath, "BezierController.log")
            : logFilePath;

        [Inject]
        void Construct(DiContainer container, GameEventBus gameEventBus, TimeLineConverter timeLineConverter,
            TimeLineSettings timeLineSettings, ActionMap actionMap)
        {
            _container = container;
            _gameEventBus = gameEventBus;
            _timeLineConverter = timeLineConverter;
            _timeLineSettings = timeLineSettings;
            _actionMap = actionMap;
        }


        private void Focus()
        {
            List<BezierPoint> focusPoints = new();
            if (selectPointsController.selectedPoints.Count > 1)
                focusPoints = selectPointsController.selectedPoints;
            else if (activePoints.Count > 1)
            {
                focusPoints = activePoints;
            }
            else
            {
                return;
            }


            float maxTime = -1;
            float minTime = float.MaxValue;
            float maxValue = float.MinValue;
            float minValue = float.MaxValue;

            foreach (var keyframe in focusPoints)
            {
                if (keyframe.BezierDragPoint._keyframe.Ticks > maxTime)
                    maxTime = (float)keyframe.BezierDragPoint._keyframe.Ticks;
                if (keyframe.BezierDragPoint._keyframe.Ticks < minTime)
                    minTime = (float)keyframe.BezierDragPoint._keyframe.Ticks;
                if (keyframe.BezierDragPoint._keyframe.GetData().GetValue() is float value && value > maxValue)
                    maxValue = value;
                if (keyframe.BezierDragPoint._keyframe.GetData().GetValue() is float value2 && value2 < minValue)
                    minValue = value2;
            }
            
            var timeDelta = maxTime / (float)Main.TICKS_PER_BEAT - minTime / (float)Main.TICKS_PER_BEAT;
            var targetWith = (rootPoints.rect.width - spacing - leftPanelAnimations.sizeDelta.x) / timeDelta;
            var result = targetWith - _timeLineSettings.DistanceBetweenBeatLines;


            var one = (_timeLineSettings.DistanceBetweenBeatLines + result) * (minTime / (float)Main.TICKS_PER_BEAT);
            var two = (_timeLineSettings.DistanceBetweenBeatLines + result) * (maxTime / (float)Main.TICKS_PER_BEAT);

            var offset = two - one - leftPanelAnimations.sizeDelta.x;

            timeLineKeyframeScroll.SetPan(result);
            scrollTimeLineKeyframe.SetPosition(-(offset / 2 + one));

            var valueDelta = maxValue - minValue;
            var targetHeight = (rootPoints.rect.height - spacing) / valueDelta;


            verticalBezierZoom.SetPan(targetHeight);

            var positionOne = minValue * targetHeight;
            var positionTwo = maxValue * targetHeight;

            var positionOffset = positionTwo - positionOne;


            SetPosition(-(positionOffset / 2 + positionOne));
        }

        private void Start()
        {
            lineDrawer?.ClearLines();
            EnsureLogDirectory();
            
            _gameEventBus.SubscribeTo((ref AddKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref RemoveKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref SelectObjectEvent _) => Build());
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent _) => Build());
            _gameEventBus.SubscribeTo((ref SelectFieldLineEvent _) => Build(), -1);
            _gameEventBus.SubscribeTo((ref DeselectFieldLineEvent _) => Build());
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.PanEvent _) => UpdatePositions());
            _gameEventBus.SubscribeTo((ref ScrollTimeLineKeyframeEvent _) => UpdatePositions());
            _gameEventBus.SubscribeTo((ref ScrollBezier _) => UpdatePositions());
            _gameEventBus.SubscribeTo((ref ZoomBezier _) => UpdatePositions());
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent _) => Clear());

            // Обработка скролла — смещаем root по Y
            _gameEventBus.SubscribeTo((ref ScrollBezier scrollEvent) =>
            {
                root.offsetMax += new Vector2(0, scrollEvent.ScrollOffset);
                root.offsetMin += new Vector2(0, scrollEvent.ScrollOffset);
                bezierLineDrawer?.UpdateBezierCurve();
            }, 1);

            // Обработка панорамирования — сохраняем значение под курсором неизменным
            _gameEventBus.SubscribeTo((ref ZoomBezier _) =>
            {
                if (!_active) return;

                float oldPan = verticalZoom.OldZoom;
                float newPan = verticalZoom.Zoom;

                Vector2 cursorLocalPos = GetCursorPosition();
                float cursorValuePos = GetCursorValuePosition(oldPan); // Значение под курсором ДО пана

                // Получаем позиции якоря ДО и ПОСЛЕ пана, БЕЗ УЧЕТА СКРОЛЛА
                float anchorPosBeforePan = GetAnchorPositionFromValue(cursorValuePos, oldPan);
                float anchorPosAfterPan = GetAnchorPositionFromValue(cursorValuePos, newPan);

                // Дельта — насколько сместилась точка под курсором из-за смены масштаба
                float delta = anchorPosBeforePan - anchorPosAfterPan;

                // Новая позиция root = старая + дельта (чтобы компенсировать смещение)
                float newPositionY = root.offsetMin.y + delta;


                SetPosition(newPositionY);
                bezierLineDrawer?.UpdateBezierCurve();
            });

            _actionMap.Editor.F.started += _ => Focus();
        }

        internal void SortPoints()
        {
            foreach (var group in groups)
            {
                group._keyframes.Sort((x, y) =>
                    x.BezierDragPoint._keyframe.Ticks.CompareTo(
                        y.BezierDragPoint._keyframe.Ticks));
            }
        }

        public void SetPosition(float position)
        {
            // Защита от NaN/Infinity
            if (float.IsNaN(position) || float.IsInfinity(position))
            {
                return;
            }

            root.offsetMax = new Vector2(0, position);
            root.offsetMin = new Vector2(0, position);
        }

        public void ActiveKeyframes(bool active)
        {
            if (selectObjectController.SelectObjects.Count <= 0) return;
            _active = active;
            if (_active == false)
            {
                lineDrawer.ClearLines();
            }

            bezierLineDrawer?.SetActive(active);

            Build();
            foreach (var group in groups)
            {
                foreach (var keyframe in group._keyframes)
                    if (keyframe != null)
                        keyframe.gameObject.SetActive(active);
            }
        }

        public Vector2 GetCursorPosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(content,
                UnityEngine.Input.mousePosition, editCameraUI, out var localPoint);
            return localPoint;
        }

        private float GetCursorValuePosition(float pan)
        {
            float scrollFactor = pan;
            if (Mathf.Approximately(scrollFactor, 0f)) return 0f;

            // Получаем позицию курсора ОТНОСИТЕЛЬНО КОРНЯ (root)
            Vector2 cursorLocalPos = GetCursorPosition();
            float cursorInRootSpace = cursorLocalPos.y - root.anchoredPosition.y; // ← ВАЖНО!

            float value = cursorInRootSpace / scrollFactor;


            return value;
        }

        // ВАЖНО: Этот метод возвращает позицию ТОЛЬКО на основе value и pan.
        // Скролл НЕ добавляется здесь, чтобы избежать двойного учета!
        private float GetAnchorPositionFromValue(float value, float pan)
        {
            float scrollFactor = pan;
            float position = value * scrollFactor; // ← Скролл НЕ добавляем!


            return position;
        }

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
            selectPointsController.Deselect();

            if (!_active || !gameObject.activeInHierarchy)
            {
                return;
            }

            // Освобождаем старые объекты
            foreach (var group in groups)
            {
                foreach (var keyframe in group._keyframes)
                {
                    if (keyframe != null)
                        Destroy(keyframe.gameObject);
                }
            }

            groups.Clear();

            bezierLineDrawer?.ClearLines();

            if (treeViewUI == null || keyframeTrackStorage == null || _timeLineConverter == null)
            {
                return;
            }

            lineDrawer.ClearBeziers();

            List<TreeNode> activeNodes = new List<TreeNode>();

            activePoints.Clear();

            foreach (var tree in treeViewUI.AnimationLineController.Lines)
            {
                if (tree?.LogicalNode == null) continue;

                Track track = keyframeTrackStorage.GetTrack(tree.LogicalNode);
                if (track == null) continue;
                List<Keyframe.Keyframe> data = ConverAverageValues(track);
                
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

                    BezierPoint point = _container.InstantiatePrefab(bizerPrefab, rootPoints)
                        .GetComponent<BezierPoint>();

                    if (point == null)
                    {
                        continue;
                    }

                    Keyframe.Keyframe prevKey = index - 1 >= 0 ? data[index - 1] : null;
                    Keyframe.Keyframe nextKey = index + 1 < data.Count ? data[index + 1] : null;

                    point.Setup(keyframeData, track.SortKeyframes, prevKey, nextKey, timeLineKeyframeScroll.Pan,
                        verticalBezierZoom.Zoom, track.Keyframes[index]);

                    // Конвертируем тики в позицию по X
                    float positionX =
                        _timeLineConverter.TicksToPositionX(keyframeData.Ticks, timeLineKeyframeScroll.Pan) +
                        content.offsetMin.x;

                    // Позиция по Y: сначала получаем "чистую" позицию от value и пана, потом ДОБАВЛЯЕМ скролл
                    float scrollFactor = verticalZoom.Zoom;
                    float basePositionY = value * scrollFactor; // ← "чистая" позиция
                    float scrolledPositionY =
                        basePositionY + verticalScroll.VerticalScroll * scrollFactor; // ← добавляем скролл ОДИН РАЗ

                    point.RectTransform.anchoredPosition = new Vector2(positionX, scrolledPositionY);

                    points.Add(point);
                }

                groups.Add(new BezierPointsGroup
                {
                    _keyframes = points, _track = data
                });
                lineDrawer.AddPoints(points, track.AnimationColor);
                activePoints.AddRange(points);
            }

            bezierLineDrawer?.UpdateBezierCurve();
        }

        private List<Keyframe.Keyframe> ConverAverageValues(Track track)
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
            foreach (var group in groups)
            {
                foreach (var keyframe in group._keyframes)
                {
                    if (keyframe != null)
                        Destroy(keyframe.gameObject);
                }
            }

            groups.Clear();

            bezierLineDrawer?.ClearLines();
            lineDrawer.ClearBeziers();
            selectPointsController.Deselect();
        }

        public void UpdatePositions()
        {
            if (!_active || !gameObject.activeInHierarchy)
            {
                return;
            }

            if (treeViewUI == null || keyframeTrackStorage == null || _timeLineConverter == null)
            {
                return;
            }
            
            // Обновляем позиции существующих точек
            foreach (var group in groups)
            {
                for (int keyframe = 0; keyframe < group._keyframes.Count; keyframe++)
                {
                    var point = group._keyframes[keyframe];
                    if (point == null || point.BezierDragPoint?._keyframe == null)
                        continue;

                    var keyframeData = point.BezierDragPoint._keyframe;
                    if (keyframeData.GetData().GetValue() is not float value)
                        continue;

                    Keyframe.Keyframe prevKey = keyframe - 1 >= 0
                        ? group._keyframes[keyframe - 1].BezierDragPoint._keyframe
                        : null;
                    Keyframe.Keyframe nextKey = keyframe + 1 < group._keyframes.Count
                        ? group._keyframes[keyframe + 1].BezierDragPoint._keyframe
                        : null;

                    point.UpdatePosition(keyframeData, prevKey, nextKey, timeLineKeyframeScroll.Pan,
                        verticalBezierZoom.Zoom);

                    // Обновляем X-позицию
                    float positionX =
                        _timeLineConverter.TicksToPositionX(keyframeData.Ticks, timeLineKeyframeScroll.Pan) +
                        content.offsetMin.x;

                    // Обновляем Y-позицию
                    float scrollFactor = verticalZoom.Zoom;
                    float basePositionY = value * scrollFactor;
                    float scrolledPositionY = basePositionY + verticalScroll.VerticalScroll * scrollFactor;

                    point.RectTransform.anchoredPosition = new Vector2(positionX, scrolledPositionY);
                }
            }

            // Перерисовываем кривые
            lineDrawer?.UpdateBezierCurve(); // или bezierLineDrawer?.UpdateBezierCurve(), в зависимости от логики
        }

        // --- Логирование в файл ---
        private void EnsureLogDirectory()
        {
            string dir = Path.GetDirectoryName(LogFile);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}

class BezierPointsGroup
{
    public List<BezierPoint> _keyframes = new();
    public List<TimeLine.Keyframe.Keyframe> _track;
}