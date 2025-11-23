using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private List<TransformComponent> _otherObjects;
        private List<TransformComponent> _allObjects;

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
            _gameEventBus.SubscribeTo(((ref SelectObjectEvent data) => SelectObject(data.Tracks)));

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

        private void OnGridPositionChanged(float newStepSize)
        {
            Log($"[OnGridPositionChanged] Received GridPositionEvent. New step size: {newStepSize:F4}");

            if (_transformComponent == null)
            {
                Log("[OnGridPositionChanged] No object is currently selected. Ignoring event.");
                return;
            }

            // ⭐⭐⭐ ОСНОВНОЕ ИЗМЕНЕНИЕ: Получаем ТЕКУЩУЮ мировую позицию объекта
            Vector2 currentWorldPosition = new Vector2(_transformComponent.transform.position.x,
                _transformComponent.transform.position.y);
            Log($"[OnGridPositionChanged] Current WORLD position of object: {currentWorldPosition:F4}");

            // ⭐ Сообщаем GridScene, что текущая мировая позиция — это наша новая "база" для расчета оффсета
            Log(
                $"[OnGridPositionChanged] Calling gridScene.SetCurrentObjectPosition({currentWorldPosition:F4}) to recalculate offset for new step size {newStepSize:F4}.");
            gridScene.SetCurrentObjectPosition(currentWorldPosition);

            // ⭐ Получаем ГЛОБАЛЬНОЕ вращение инструмента (или объекта, если в локальной системе)
            Quaternion currentToolGlobalRotationInverse = Quaternion.Inverse(_rectTransformPositionTool.rotation);
            Log(
                $"[OnGridPositionChanged] Current tool global rotation inverse: {currentToolGlobalRotationInverse.eulerAngles:F2}");

            // ⭐ Рассчитываем новую снапнутую позицию на основе ТЕКУЩЕЙ мировой позиции и НОВОГО шага
            Vector2 newSnappedPosition =
                gridScene.PositionFloatSnapToGrid(currentWorldPosition, currentToolGlobalRotationInverse);
            Log(
                $"[OnGridPositionChanged] New snapped WORLD position (after grid step change): {newSnappedPosition:F4}");

            // ⭐ Обновляем кэш последней снапнутой позиции (теперь в мировых координатах для внутренней логики)
            _lastSnappedPosition = newSnappedPosition;

            // --- Синхронизируем объект ---
            _transformComponent.XPosition.Value = newSnappedPosition.x;
            _transformComponent.YPosition.Value = newSnappedPosition.y;
            Log(
                $"[OnGridPositionChanged] Applied new snapped position to TransformComponent: X={newSnappedPosition.x:F4}, Y={newSnappedPosition.y:F4}");

            // --- Синхронизируем UI инструмента ---
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _mainObjects.ToolRectTransform,
                    camera.WorldToScreenPoint(newSnappedPosition),
                    camera,
                    out var snappedLocalPoint))
            {
                _rectTransformPositionTool.anchoredPosition = snappedLocalPoint;
                Log($"[OnGridPositionChanged] [SYNC TOOL] Tool position synced to: {snappedLocalPoint:F4}");
            }
            else
            {
                Log("[OnGridPositionChanged] WARNING: Failed to sync tool position after grid step change!");
            }
        }


        private void SelectObject(List<TrackObjectData> data)
        {
            Log($"--- SelectObject START for: {data} ---");

            //Собираем все остальные объекты
            _otherObjects = data
                .Select(i => i.sceneObject.GetComponent<TransformComponent>())
                .Where(comp => comp != null)
                .ToList();

            _allObjects = new List<TransformComponent>(_otherObjects);
            
            //Получаем последний выбранный и удаляем из списка прочих
            _transformComponent = _otherObjects[^1];
            _otherObjects.Remove(_transformComponent);
            
            if (_transformComponent == null)
            {
                Log("WARNING: Selected object has no TransformComponent!");
                Log("--- SelectObject END ---");
                return;
            }

            // ⭐ Получаем ЛОКАЛЬНУЮ позицию для GridScene
            Vector2 localPositionForGrid =
                new Vector2(_transformComponent.XPosition.Value, _transformComponent.YPosition.Value);
            Log($"[SelectObject] Local position for GridScene: {localPositionForGrid:F4}");

            // ⭐ Получаем ГЛОБАЛЬНУЮ позицию для позиционирования UI инструмента
            Vector2 worldPositionForUI = new Vector2(_transformComponent.transform.position.x,
                _transformComponent.transform.position.y);
            Log($"[SelectObject] World position for UI Tool: {worldPositionForUI:F4}");

            print(worldPositionForUI);
            worldPositionForUI = GetCenter.GetSelectionCenter(_allObjects.Select(i => i.transform).ToList()); //todo test
            print(worldPositionForUI);

            // Сообщаем GridScene текущую локальную позицию объекта (для расчета оффсета)
            Log($"[SelectObject] Calling gridScene.SetCurrentObjectPosition({localPositionForGrid:F4})");
            gridScene.SetCurrentObjectPosition(localPositionForGrid);

            // Кэшируем последнюю снапнутую позицию (локальную, для гистерезиса)
            _lastSnappedPosition = localPositionForGrid;
            Log($"[SelectObject] Cached _lastSnappedPosition (local): {_lastSnappedPosition:F4}");

            // Назначаем локальную позицию обратно в компонент (без снапа, как и требовалось)
            // _transformComponent.XPosition.Value = localPositionForGrid.x;
            // _transformComponent.YPosition.Value = localPositionForGrid.y;
            Log(
                $"[SelectObject] Assigned local position to TransformComponent: X={localPositionForGrid.x:F4}, Y={localPositionForGrid.y:F4}");

            // --- Обновление UI инструмента с использованием ГЛОБАЛЬНОЙ позиции ---
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _mainObjects.ToolRectTransform,
                    camera.WorldToScreenPoint(worldPositionForUI), // ⭐ Используем МИРОВУЮ позицию!
                    camera,
                    out var localPoint))
            {
                Log("ERROR: RectTransformUtility.ScreenPointToLocalPointInRectangle FAILED!");
                Log("--- SelectObject END ---");
                return;
            }

            Log(
                $"[SelectObject] Converted world position {worldPositionForUI:F4} to tool local point: {localPoint:F4}");
            _rectTransformPositionTool.anchoredPosition = localPoint;

            // Устанавливаем вращение инструмента
            float targetRotation =
                coordinateSystem.IsGlobal ? 0f : _transformComponent.transform.rotation.eulerAngles.z;
            _rectTransformPositionTool.rotation = Quaternion.Euler(0f, 0f, targetRotation);
            Log($"[SelectObject] Set tool rotation to: {targetRotation:F2} degrees");

            // Подписываемся на события
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
            Log(
                $"[UpdateObjectFromTool] Applied inverse rotation: {rotationAngle:F2} degrees (Euler: {inverseRotation.eulerAngles:F2})");

            // 3. Снапаем с оффсетом — GridScene сам всё учтёт!
            Log(
                $"[UpdateObjectFromTool] Calling gridScene.PositionFloatSnapToGrid({smoothWorldPosition:F4}, {inverseRotation.eulerAngles:F2})");
            
            var oldPosition = GetCenter.GetSelectionCenter(_allObjects.Select(i => i.transform).ToList()); //Сохраняем старую позицию
            
            Vector2 candidateSnappedPosition = gridScene.PositionFloatSnapToGrid(smoothWorldPosition, inverseRotation);
            
            var differentPosition = candidateSnappedPosition - oldPosition; //Высчитываем разницу позиций
            
            Log($"[UpdateObjectFromTool] Candidate snapped position FROM GRIDSCENE: {candidateSnappedPosition:F4}");

            
            //Добавляем позицию ко всем объектам
            foreach (var otherObject in _allObjects)
            {
                otherObject.XPosition.Value += differentPosition.x;
                otherObject.YPosition.Value += differentPosition.y;
            }

            // 4. Гистерезис
            // float distanceToLastSnap = Vector2.Distance(candidateSnappedPosition, _lastSnappedPosition);
            // Log(
            //     $"[UpdateObjectFromTool] Distance to last snap: {distanceToLastSnap:F4}, Hysteresis threshold: {_snapHysteresis:F4}");
            //
            // if (distanceToLastSnap > _snapHysteresis)
            // {
            //     _lastSnappedPosition = candidateSnappedPosition;
            //     Log($"[UpdateObjectFromTool] ✅ Accepted new snap position: {_lastSnappedPosition:F4}");
            // }
            // else
            // {
            //     candidateSnappedPosition = _lastSnappedPosition;
            //     Log($"[UpdateObjectFromTool] 🚫 Ignored candidate, keeping last snap: {_lastSnappedPosition:F4}");
            // }

            // 5. Применяем позицию
            // _transformComponent.XPosition.Value = candidateSnappedPosition.x;
            // _transformComponent.YPosition.Value = candidateSnappedPosition.y;
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
            Log("=== PositionController Session Ended ===");
            _logWriter?.Close();
            _logWriter?.Dispose();
        }
    }
}