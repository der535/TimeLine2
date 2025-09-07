using System;
using System.IO;
using System.Text;
using EventBus;
using TimeLine.EventBus.Events.Grid; // ⭐ Добавлен импорт
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class PositionController : MonoBehaviour
    {
        [SerializeField] private CoordinateSystem coordinateSystem;
        [SerializeField] private PositionTool positionTool;
        [SerializeField] private Camera camera;
        [SerializeField] private GridScene gridScene;
        [SerializeField] private Canvas canvas;

        [SerializeField] private RectTransform _rectTransformPositionTool;
        private MainObjects _mainObjects;
        private GameEventBus _gameEventBus;

        private TransformComponent _transformComponent;

        private bool _isUpdatingFromTool = false;
        private bool _isUpdatingFromObject = false;
        private bool _isSubscribedToGridEvents = false; // ⭐ Флаг подписки

        // ⭐ НОВОЕ: последняя снапнутая позиция (для устранения дребезга)
        private Vector2 _lastSnappedPosition;

        // ⭐ НОВОЕ: порог гистерезиса — не меняем точку, пока не отошли от последней на это расстояние
        [SerializeField] private float _snapHysteresis = 0.15f; // Настрой в инспекторе под свою сетку

        // 🔽 Укажи путь в Инспекторе, например: "C:/Temp/PositionController_Log.txt"
        [SerializeField] private string _logPath = "C:/Temp/PositionController_Log.txt";
        [SerializeField] private bool _enableLogging = true; // ⭐ Галочка для включения/выключения логов
        private StreamWriter _logWriter;

        [Inject]
        private void Construct(GameEventBus eventBus, MainObjects mainObjects)
        {
            _gameEventBus = eventBus;
            _mainObjects = mainObjects;
        }

        private void Awake()
        {
            // ⭐ Опционально: обернуть инициализацию логгера
            if (_enableLogging)
            {
                InitializeLogger();
            }
            Log("=== PositionController Awake ===");
        }

        private void InitializeLogger()
        {
            // ⭐ Проверка, включено ли логирование
            if (!_enableLogging) return;

            if (string.IsNullOrWhiteSpace(_logPath))
            {
                Debug.LogError("Log path is not set! Please assign a valid path in Inspector.");
                return;
            }

            try
            {
                string directory = Path.GetDirectoryName(_logPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _logWriter = new StreamWriter(_logPath, false, Encoding.UTF8); // false = перезапись
                Log($"Logger initialized. File: {_logPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create log file at '{_logPath}': {e.Message}");
            }
        }

        private void Log(string message)
        {
            // ⭐ Проверка, включено ли логирование
            if (!_enableLogging) return;

            string logLine = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            Debug.Log(logLine);
            
            // Проверяем и writer тоже, на всякий случай
            if (_logWriter != null) 
            {
                _logWriter?.WriteLine(logLine);
                _logWriter?.Flush();
            }
        }

        private void Start()
        {
            Log("Subscribing to SelectObjectEvent...");
            _gameEventBus.SubscribeTo(((ref SelectObjectEvent data) => SelectObject(data.Track.sceneObject)));

            // ⭐ Подписка на GridPositionEvent
            Log("Subscribing to GridPositionEvent...");
            _gameEventBus.SubscribeTo(((ref GridPositionEvent data) => OnGridPositionChanged(data.StepSize)));
            _isSubscribedToGridEvents = true;

            coordinateSystem.OnCoordinateChanged += b =>
            {
                Log(
                    $"CoordinateSystem changed to {(b ? "Global" : "Local")}, setting tool rotation to {_transformComponent?.ZRotation.Value:F2}");
                _rectTransformPositionTool.rotation =
                    Quaternion.Euler(0f, 0f, b ? 0f : _transformComponent?.ZRotation.Value ?? 0f);
            };
        }

        // ⭐ Новый метод-обработчик события изменения шага сетки
        private void OnGridPositionChanged(float newStepSize)
        {
            Log($"[OnGridPositionChanged] Received GridPositionEvent. New step size: {newStepSize:F4}");

            if (_transformComponent == null)
            {
                Log("[OnGridPositionChanged] No object is currently selected. Ignoring event.");
                return;
            }

            // ⭐ Используем _lastSnappedPosition как "фиксированную" точку для пересчета оффсета.
            // Это та позиция, которая была последней применена к объекту.
            Vector2 fixedSnappedPosition = _lastSnappedPosition;
            Log($"[OnGridPositionChanged] Using _lastSnappedPosition ({fixedSnappedPosition:F4}) as the fixed point for grid offset recalculation.");

            Log($"[OnGridPositionChanged] Calling gridScene.SetCurrentObjectPosition({fixedSnappedPosition:F4}) to recalculate offset for new step size {newStepSize:F4}.");
            gridScene.SetCurrentObjectPosition(fixedSnappedPosition);

            // ⭐ После пересчета оффсета в GridScene, _lastSnappedPosition по сути остается "правильным"
            // для новой сетки. Тем не менее, для полной уверенности, пересчитаем его явно.
            // Получаем текущую "плавную" позицию инструмента
            Vector2 currentToolWorldPosition = TimeLineConverter.ConvertAnchorToWorldPosition(_rectTransformPositionTool, canvas);
            // Снапаем её с новым оффсетом и шагом
            float rotationAngle = -_rectTransformPositionTool.localEulerAngles.z;
            Quaternion currentToolRotationInverse = Quaternion.Euler(0, 0, rotationAngle);

            Log($"[OnGridPositionChanged] Recalculating _lastSnappedPosition based on current tool position ({currentToolWorldPosition:F4}) and new grid settings.");
            Vector2 newSnappedPosition = gridScene.PositionFloatSnapToGrid(currentToolWorldPosition, currentToolRotationInverse);

            // Обновляем _lastSnappedPosition
            _lastSnappedPosition = newSnappedPosition;
            Log($"[OnGridPositionChanged] _lastSnappedPosition updated to new snapped position: {_lastSnappedPosition:F4} based on new grid offset and step size.");

            // ⭐ ВАЖНО: Убедиться, что UI инструмента также синхронизирован с новой позицией.
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _mainObjects.ToolRectTransform,
                    camera.WorldToScreenPoint(_lastSnappedPosition),
                    camera,
                    out var snappedLocalPoint))
            {
                _rectTransformPositionTool.anchoredPosition = snappedLocalPoint;
                Log($"[OnGridPositionChanged] [SYNC TOOL] Tool position synced to: {snappedLocalPoint:F4} after grid step change.");
            }
            else
            {
                Log("[OnGridPositionChanged] WARNING: Failed to sync tool position after grid step change!");
            }
        }


        private void SelectObject(GameObject data)
        {
            Log($"--- SelectObject START for: {data?.name ?? "NULL"} ---");

            if (_transformComponent != null)
            {
                Log("Unsubscribing from previous TransformComponent (if any) — currently commented out.");
            }

            _transformComponent = data?.GetComponent<TransformComponent>();
            if (_transformComponent == null)
            {
                Log("WARNING: Selected object has no TransformComponent!");
                Log("--- SelectObject END ---");
                return;
            }

            Vector2 originalPosition = new Vector2(_transformComponent.XPosition.Value, _transformComponent.YPosition.Value);
            Log($"[SelectObject] Original object position from TransformComponent: {originalPosition:F4}");

            // ⭐⭐⭐ ВАЖНО: Сообщаем GridScene, какой объект сейчас выбран.
            Log($"[SelectObject] Calling gridScene.SetCurrentObjectPosition({originalPosition:F4})");
            gridScene.SetCurrentObjectPosition(originalPosition);

            // ⭐ Снапаем с учётом нового оффсета
            Log($"[SelectObject] Calling gridScene.PositionFloatSnapToGrid({originalPosition:F4}, identity) to get snapped position with new offset.");
            Vector2 snappedWorldPosition = gridScene.PositionFloatSnapToGrid(originalPosition, Quaternion.identity);
            Log($"[SelectObject] Snapped world position (with NEW GridScene offset): {snappedWorldPosition:F4}");

            _transformComponent.XPosition.Value = snappedWorldPosition.x;
            _transformComponent.YPosition.Value = snappedWorldPosition.y;

            // ⭐ Обновляем _lastSnappedPosition в PositionController
            _lastSnappedPosition = snappedWorldPosition;
            Log($"[SelectObject] Assigned final position to TransformComponent and cached snapped position as _lastSnappedPosition: {_lastSnappedPosition:F4}");

            // --- Обновление UI инструмента ---
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _mainObjects.ToolRectTransform,
                    camera.WorldToScreenPoint(snappedWorldPosition),
                    camera,
                    out var localPoint))
            {
                Log("ERROR: RectTransformUtility.ScreenPointToLocalPointInRectangle FAILED!");
                Log("--- SelectObject END ---");
                return;
            }

            Log($"[SelectObject] Converted snapped world position {snappedWorldPosition:F4} to tool local point: {localPoint:F4}");
            _rectTransformPositionTool.anchoredPosition = localPoint;

            float targetRotation = coordinateSystem.IsGlobal ? 0f : _transformComponent.ZRotation.Value;
            _rectTransformPositionTool.rotation = Quaternion.Euler(0f, 0f, targetRotation);
            Log($"[SelectObject] Set tool rotation to: {targetRotation:F2} degrees");

            positionTool.OnChangePosition -= UpdateObjectFromTool;
            positionTool.OnChangePosition += UpdateObjectFromTool;
            Log("[SelectObject] Subscribed UpdateObjectFromTool to PositionTool.OnChangePosition");
            Log("--- SelectObject END ---");
        }

        private void UpdateObjectFromTool(RectTransform toolTransform)
        {
            if (_isUpdatingFromObject || _transformComponent == null) return;
            _isUpdatingFromTool = true;
            Log("=== UpdateObjectFromTool START (snap with hysteresis) ===");

            // 1. Конвертируем позицию инструмента в мировые координаты
            Vector2 smoothWorldPosition = TimeLineConverter.ConvertAnchorToWorldPosition(toolTransform, canvas);
            Log($"[UpdateObjectFromTool] Converted tool position to world: {smoothWorldPosition:F4}");

            // 2. Применяем вращение
            float rotationAngle = -toolTransform.localEulerAngles.z;
            Quaternion inverseRotation = Quaternion.Euler(0, 0, rotationAngle);
            Log($"[UpdateObjectFromTool] Applied inverse rotation: {rotationAngle:F2} degrees (Euler: {inverseRotation.eulerAngles:F2})");

            // 3. Снапаем с оффсетом — GridScene сам всё учтёт!
            Log($"[UpdateObjectFromTool] Calling gridScene.PositionFloatSnapToGrid({smoothWorldPosition:F4}, {inverseRotation.eulerAngles:F2})");
            Vector2 candidateSnappedPosition = gridScene.PositionFloatSnapToGrid(smoothWorldPosition, inverseRotation);
            Log($"[UpdateObjectFromTool] Candidate snapped position FROM GRIDSCENE: {candidateSnappedPosition:F4}");

            // 4. Гистерезис
            float distanceToLastSnap = Vector2.Distance(candidateSnappedPosition, _lastSnappedPosition);
            Log($"[UpdateObjectFromTool] Distance to last snap: {distanceToLastSnap:F4}, Hysteresis threshold: {_snapHysteresis:F4}");

            if (distanceToLastSnap > _snapHysteresis)
            {
                _lastSnappedPosition = candidateSnappedPosition;
                Log($"[UpdateObjectFromTool] ✅ Accepted new snap position: {_lastSnappedPosition:F4}");
            }
            else
            {
                candidateSnappedPosition = _lastSnappedPosition;
                Log($"[UpdateObjectFromTool] 🚫 Ignored candidate, keeping last snap: {_lastSnappedPosition:F4}");
            }

            // 5. Применяем позицию
            _transformComponent.XPosition.Value = candidateSnappedPosition.x;
            _transformComponent.YPosition.Value = candidateSnappedPosition.y;
            Log(
                $"[UpdateObjectFromTool] Assigned final position to TransformComponent: X={candidateSnappedPosition.x:F4}, Y={candidateSnappedPosition.y:F4}");

            // 6. Синхронизируем инструмент
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _mainObjects.ToolRectTransform,
                    camera.WorldToScreenPoint(candidateSnappedPosition),
                    camera,
                    out var snappedLocalPoint))
            {
                _rectTransformPositionTool.anchoredPosition = snappedLocalPoint;
                Log($"[UpdateObjectFromTool] [SYNC TOOL] Tool position synced to: {snappedLocalPoint:F4}");
            }
            else
            {
                Log("[UpdateObjectFromTool] WARNING: Failed to sync tool position after snap!");
            }

            _isUpdatingFromTool = false;
            Log("=== UpdateObjectFromTool END ===");
        }

        // ⭐ В метод OnDestroy, добавить отписку (хороший тон):
        private void OnDestroy()
        {
            // if (_isSubscribedToGridEvents && _gameEventBus != null)
            // {
            //     Log("Unsubscribing from GridPositionEvent...");
            //     // Note: EventBus usually handles unsubscribe automatically on object destruction
            //     // or you might need to call a specific unsubscribe method if available.
            //     // _gameEventBus.Unsubscribe<GridPositionEvent>(OnGridPositionChanged); 
            //     // If such a method exists.
            // }

            Log("=== PositionController Session Ended ===");
            _logWriter?.Close();
            _logWriter?.Dispose();
        }
    }
}