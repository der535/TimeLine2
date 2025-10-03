using System;
using System.Collections.Generic;
using System.IO;
using EventBus;
using NaughtyAttributes;
using TimeLine;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
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
        [FormerlySerializedAs("_content")] [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform rootPoints;
        [SerializeField] private BezierLineDrawer bezierLineDrawer;
        [SerializeField] private BezierLineDrawer lineDrawer; // Возможно дублирует bezierLineDrawer? Проверьте.
        [SerializeField] private VerticalBezierScroll verticalScroll;
        [SerializeField] private VerticalBezierPan verticalPan;
        [FormerlySerializedAs("_timeLineKeyframeScroll")] [SerializeField] private TimeLineKeyframeScroll timeLineKeyframeScroll;
        [SerializeField] private Camera camera;
        [SerializeField] private RectTransform centerLine;
        [SerializeField] private RectTransform root;
        [SerializeField] private float spacing = 30;
        [SerializeField] private ScrollTimeLineKeyframe ScrollTimeLineKeyframe;
        [SerializeField] private VerticalBezierPan verticalBezierPan;
        [Header("Logging")]
        [SerializeField] private string logFilePath = ""; // Укажите путь вручную, например: "C:/Temp/BezierController.log"
        [SerializeField] private SelectFieldLineController selectFieldLineController;

        private TimeLineSettings _timeLineSettings;
        private TimeLineConverter _timeLineConverter;
        private List<BezierPointsGroup> groups = new();
        private bool _active;

        private DiContainer _container;
        private GameEventBus _gameEventBus;

        private string LogFile => string.IsNullOrEmpty(logFilePath) ? Path.Combine(Application.persistentDataPath, "BezierController.log") : logFilePath;

        [Inject]
        void Construct(DiContainer container, GameEventBus gameEventBus, TimeLineConverter timeLineConverter, TimeLineSettings timeLineSettings)
        {
            _container = container;
            _gameEventBus = gameEventBus;
            _timeLineConverter = timeLineConverter;
            _timeLineSettings = timeLineSettings;
        }

        private Track tracksaved;

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.J))
            {
                Focus();
            }
        }

        [Button]
        private void Focus()
        {
            float maxTime = -1;
            float minTime = float.MaxValue;
            float maxValue = float.MinValue;
            float minValue = float.MaxValue;
            
            foreach (var keyframe in tracksaved.Keyframes)
            {
                if(keyframe.Ticks > maxTime) maxTime = (float)keyframe.Ticks;
                if(keyframe.Ticks < minTime) minTime = (float)keyframe.Ticks;
                if(keyframe.GetData().GetValue() is float value && value > maxValue) maxValue = value;
                if(keyframe.GetData().GetValue() is float value2 && value2 < minValue) minValue = value2;
            }
            
            // print(maxTime);
            // print(minTime);
            //
            // print(maxValue);
            // print(minValue);
            //
            // print(rootPoints.rect.width);
            // print(rootPoints.rect.height);

            var panHorizontal = Math.Abs((minTime - maxTime) / (rootPoints.rect.width - spacing) * (minTime - maxTime));
            var panVertical = Math.Abs((rootPoints.rect.height - spacing) / (minValue - maxValue));
            
            // print(panHorizontal);
            // print(panVertical);
            
            var timeDelta = maxTime / (float)Main.TICKS_PER_BEAT - minTime / (float)Main.TICKS_PER_BEAT;
            var targetWith = (rootPoints.rect.width - spacing) / timeDelta;
            var result = targetWith - _timeLineSettings.DistanceBetweenBeatLines;
            
            // print(timeDelta);
            // print(targetWith);
            // print(result);
            
            var one = (_timeLineSettings.DistanceBetweenBeatLines + result) * (minTime / (float)Main.TICKS_PER_BEAT);
            var two = (_timeLineSettings.DistanceBetweenBeatLines + result) * (maxTime / (float)Main.TICKS_PER_BEAT);

            var offset = two - one;
            
            timeLineKeyframeScroll.SetPan(result);
            ScrollTimeLineKeyframe.SetPosition(-(offset / 2 + one));
            
            var valueDelta = maxValue - minValue;
            var targetHeight = (rootPoints.rect.height - spacing) / valueDelta;
            // var vertialpan = targetHeight; 
            
            verticalBezierPan.SetPan(targetHeight);
            
            var positionOne = minValue * targetHeight;
            var positionTwo = maxValue * targetHeight;
            
            var positionOffset = positionTwo - positionOne;
            
            print(positionOne);
            print(positionTwo);
            print(positionOffset);
            print(-(positionOffset / 2 + positionOne));
            
            SetPosition(-(positionOffset / 2 + positionOne));
        }

        private void Start()
        {
            lineDrawer?.ClearLines();
            EnsureLogDirectory();

            LogMessage("=== BezierController Started ===");

            _gameEventBus.SubscribeTo((ref AddKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref RemoveKeyframeEvent _) => Build());
            _gameEventBus.SubscribeTo((ref SelectObjectEvent _) => Build());
            _gameEventBus.SubscribeTo((ref EventBus.Events.KeyframeTimeLine.PanEvent _) => UpdatePositions());
            _gameEventBus.SubscribeTo((ref ScrollTimeLineKeyframeEvent _) => UpdatePositions());
            _gameEventBus.SubscribeTo((ref ScrollBezier _) => UpdatePositions());
            _gameEventBus.SubscribeTo((ref PanBezier _) => UpdatePositions());

            // Обработка скролла — смещаем root по Y
            _gameEventBus.SubscribeTo((ref ScrollBezier scrollEvent) =>
            {
                LogMessage($"[Scroll] ScrollOffset: {scrollEvent.ScrollOffset}");
                root.offsetMax += new Vector2(0, scrollEvent.ScrollOffset);
                root.offsetMin += new Vector2(0, scrollEvent.ScrollOffset);
                bezierLineDrawer?.UpdateBezierCurve();
            }, 1);

            // Обработка панорамирования — сохраняем значение под курсором неизменным
            _gameEventBus.SubscribeTo((ref PanBezier _) =>
            {
                float oldPan = verticalPan.OldPan;
                float newPan = verticalPan.Pan;

                Vector2 cursorLocalPos = GetCursorPosition();
                float cursorValuePos = GetCursorValuePosition(oldPan); // Значение под курсором ДО пана

                // Получаем позиции якоря ДО и ПОСЛЕ пана, БЕЗ УЧЕТА СКРОЛЛА
                float anchorPosBeforePan = GetAnchorPositionFromValue(cursorValuePos, oldPan);
                float anchorPosAfterPan = GetAnchorPositionFromValue(cursorValuePos, newPan);

                // Дельта — насколько сместилась точка под курсором из-за смены масштаба
                float delta = anchorPosBeforePan - anchorPosAfterPan;

                // Новая позиция root = старая + дельта (чтобы компенсировать смещение)
                float newPositionY = root.offsetMin.y + delta;

                LogMessage($"[Pan] OldPan: {oldPan}, NewPan: {newPan}");
                LogMessage($"[Pan] CursorLocalY: {cursorLocalPos.y}, CursorValuePos: {cursorValuePos}");
                LogMessage($"[Pan] AnchorBefore (no scroll): {anchorPosBeforePan}, AnchorAfter (no scroll): {anchorPosAfterPan}, Delta: {delta}");
                LogMessage($"[Pan] CurrentRootY: {root.offsetMin.y}, Setting Root Y to: {newPositionY}");

                SetPosition(newPositionY);
                bezierLineDrawer?.UpdateBezierCurve();
            });
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
                LogMessage($"[ERROR] SetPosition received invalid value: {position}");
                return;
            }

            root.offsetMax = new Vector2(0, position);
            root.offsetMin = new Vector2(0, position);
            LogMessage($"[SetPosition] Root Y set to: {position}");
        }

        public void ActiveKeyframes(bool active)
        {
            _active = active;
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
                UnityEngine.Input.mousePosition, camera, out var localPoint);
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

            LogMessage($"[GetCursorValuePosition] Pan: {pan}, ScrollFactor: {scrollFactor}, CursorInRootSpace: {cursorInRootSpace}, ResultValue: {value}");

            return value;
        }

        // ВАЖНО: Этот метод возвращает позицию ТОЛЬКО на основе value и pan.
        // Скролл НЕ добавляется здесь, чтобы избежать двойного учета!
        private float GetAnchorPositionFromValue(float value, float pan)
        {
            float scrollFactor = pan;
            float position = value * scrollFactor; // ← Скролл НЕ добавляем!

            LogMessage($"[GetAnchorPositionFromValue] Value: {value}, Pan: {pan}, ScrollFactor: {scrollFactor}, ResultPosition (no scroll): {position}");

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
            if (!_active || !gameObject.activeInHierarchy)
            {
                LogMessage("[Build] Skipping build: not active or inactive in hierarchy.");
                return;
            }

            LogMessage("[Build] Starting rebuild...");

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
                LogMessage("[Build] WARNING: Some dependencies are not assigned.");
                return;
            }
            
            lineDrawer.ClearBeziers();
            
            List<TreeNode> activeNodes = new List<TreeNode>();
            
            foreach (var tree in treeViewUI.AnimationLineController.Lines)
            {
                if (tree?.LogicalNode == null) continue;

                Track track = keyframeTrackStorage.GetTrack(tree.LogicalNode);

                // print(selectFieldLineController.CheckActive(tree.LogicalNode));

                if (!activeNodes.Contains(tree.LogicalNode))
                {
                    print(activeNodes.Count);

                    foreach (var node in activeNodes)
                    {
                        print(node.Name);
                    }
                    
                    if (selectFieldLineController.CheckActive(tree.LogicalNode) == false)
                    {
                        continue;
                    }

                    // print("AddRange");
                    AddList(ref activeNodes, tree.LogicalNode);
                }

                
                tracksaved = track;
                if (track == null) continue;

                List<BezierPoint> points = new List<BezierPoint>();

                for (var index = 0; index < track.Keyframes.Count; index++)
                {
                    var keyframeData = track.Keyframes[index];
                    if (keyframeData.GetData().GetValue() is not float value) continue;

                    BezierPoint point = _container.InstantiatePrefab(bizerPrefab, rootPoints)
                        .GetComponent<BezierPoint>();

                    if (point == null)
                    {
                        LogMessage("[Build] ERROR: Failed to instantiate BezierPoint prefab.");
                        continue;
                    }

                    Keyframe.Keyframe prevKey = index - 1 >= 0 ? track.Keyframes[index - 1] : null;
                    Keyframe.Keyframe nextKey = index + 1 < track.Keyframes.Count ? track.Keyframes[index+1] : null;
                    
                    point.Setup(keyframeData, track.SortKeyframes, prevKey, nextKey, timeLineKeyframeScroll.Pan, verticalBezierPan.Pan);

                    // Конвертируем тики в позицию по X
                    float positionX =
                        _timeLineConverter.TicksToPositionX(keyframeData.Ticks, timeLineKeyframeScroll.Pan) +
                        content.offsetMin.x;

                    // Позиция по Y: сначала получаем "чистую" позицию от value и пана, потом ДОБАВЛЯЕМ скролл
                    float scrollFactor = verticalPan.Pan;
                    float basePositionY = value * scrollFactor; // ← "чистая" позиция
                    float scrolledPositionY =
                        basePositionY + verticalScroll.VerticalScroll * scrollFactor; // ← добавляем скролл ОДИН РАЗ

                    point.RectTransform.anchoredPosition = new Vector2(positionX, scrolledPositionY);
                    LogMessage(
                        $"[Build] Placed point at X: {positionX:F2}, Y: {scrolledPositionY:F2} for value: {value}");

                    points.Add(point);
                }
                groups.Add(new BezierPointsGroup
                {
                    _keyframes = points, _track = track
                });
                lineDrawer.AddPoints(points, track.AnimationColor);
            }

            bezierLineDrawer?.UpdateBezierCurve();

            LogMessage($"[Build] Completed. Total points: {groups.Count}");
        }
        
        public void UpdatePositions()
        {
            if (!_active || !gameObject.activeInHierarchy)
            {
                LogMessage("[UpdatePositions] Skipping: not active or inactive in hierarchy.");
                return;
            }

            if (treeViewUI == null || keyframeTrackStorage == null || _timeLineConverter == null)
            {
                LogMessage("[UpdatePositions] WARNING: Some dependencies are not assigned.");
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

                    Keyframe.Keyframe prevKey = keyframe - 1 >= 0 ? group._keyframes[keyframe - 1].BezierDragPoint._keyframe : null;
                    Keyframe.Keyframe nextKey = keyframe + 1 < group._keyframes.Count ? group._keyframes[keyframe + 1].BezierDragPoint._keyframe : null;
                    
                    point.UpdatePosition(keyframeData, prevKey, nextKey, timeLineKeyframeScroll.Pan, verticalBezierPan.Pan);
                    
                    // Обновляем X-позицию
                    float positionX = _timeLineConverter.TicksToPositionX(keyframeData.Ticks, timeLineKeyframeScroll.Pan) +
                                      content.offsetMin.x;

                    // Обновляем Y-позицию
                    float scrollFactor = verticalPan.Pan;
                    float basePositionY = value * scrollFactor;
                    float scrolledPositionY = basePositionY + verticalScroll.VerticalScroll * scrollFactor;

                    point.RectTransform.anchoredPosition = new Vector2(positionX, scrolledPositionY);
                }
            }

            // Перерисовываем кривые
            lineDrawer?.UpdateBezierCurve(); // или bezierLineDrawer?.UpdateBezierCurve(), в зависимости от логики
            LogMessage($"[UpdatePositions] Updated positions for {groups.Count} points.");
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

        private void LogMessage(string message)
        {
            return;
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logLine = $"[{timestamp}] {message}";

                File.AppendAllText(LogFile, logLine + Environment.NewLine);
                // Также выводим в консоль для удобства
                Debug.Log($"[BezierController] {message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BezierController] Failed to write log: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            LogMessage("=== BezierController Destroyed ===");
        }
    }
}

class BezierPointsGroup
{
    public List<BezierPoint> _keyframes = new(); 
    public Track _track;
}