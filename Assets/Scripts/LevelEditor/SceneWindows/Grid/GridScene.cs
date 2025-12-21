using System;
using System.Globalization;
using System.IO;
using EventBus;
using TimeLine.EventBus.Events.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using Debug = UnityEngine.Debug; // Ensure we use Unity's Debug

namespace TimeLine
{
    public class GridScene : MonoBehaviour
    {
        [FormerlySerializedAs("_gridSize")] [SerializeField]
        private float _positionStepSize = 1f;

        [SerializeField] private float _rotateStep = 90f;
        [Space] 
        [SerializeField] private TMP_InputField gridSizeInputField;
        [SerializeField] private TMP_InputField gridRotateSizeInputField;

        [Space]
        // 🔽 Укажите полный путь или используйте Application.persistentDataPath в коде
        [SerializeField]
        private string logFilePath = "Logs/grid_scene_log.txt";

        [SerializeField] private bool _enableLogging = true; // ⭐ Галочка для включения/выключения логов

        private GameEventBus _gameEventBus;
        private Vector2 _gridOffset = Vector2.zero;

        // ⭐ Для логирования
        private string _fullLogPath;
        private StreamWriter _logWriter;

        // ⭐ Сохраняем последнюю позицию объекта для пересчета оффсета при смене шага
        private Vector2 _lastObjectPosition = Vector2.zero;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        public float PositionStepSize => _positionStepSize;
        public Vector2 GridOffset => _gridOffset; // Только для чтения

        internal (float, float) GetGridSize()
        {
            return (float.Parse(gridSizeInputField.text, CultureInfo.InvariantCulture), float.Parse(gridRotateSizeInputField.text, CultureInfo.InvariantCulture));
        }

        internal void SetGridSize(float gridSize, float rotateGridSize)
        {
            gridSizeInputField.text = gridSize.ToString(CultureInfo.InvariantCulture);
            gridRotateSizeInputField.text = rotateGridSize.ToString(CultureInfo.InvariantCulture);
            gridSizeInputField.onValueChanged.Invoke(gridSizeInputField.text);
            gridRotateSizeInputField.onValueChanged.Invoke(gridRotateSizeInputField.text);
        }

        private void Start()
        {
            // ⭐ Опционально: обернуть инициализацию логгера
            if (_enableLogging)
            {
                InitializeLogging();
            }
            
            LogMessage("GridScene started");
            LogMessage($"Initial position step size: {_positionStepSize:F4}");
            LogMessage($"Initial rotate step: {_rotateStep:F4}");

            gridSizeInputField.text = _positionStepSize.ToString(CultureInfo.InvariantCulture);
            gridRotateSizeInputField.text = _rotateStep.ToString(CultureInfo.InvariantCulture);

            gridSizeInputField.onEndEdit.AddListener(arg0 =>
            {
                if (string.IsNullOrWhiteSpace(arg0) || !float.TryParse(arg0, NumberStyles.Float, CultureInfo.InvariantCulture, out float result) || result <= 0)
                {
                    // Восстанавливаем предыдущее валидное значение или значение по умолчанию
                    float fallback = _positionStepSize > 0 ? _positionStepSize : 1f;
                    LogMessage($"Invalid or empty grid size input: '{arg0}'. Resetting to {fallback:F4}");
                    gridSizeInputField.text = fallback.ToString(CultureInfo.InvariantCulture);
                    // Не вызываем SetPositionStepSize, если значение не изменилось
                    if (!Mathf.Approximately(fallback, _positionStepSize))
                    {
                        SetPositionStepSize(fallback);
                    }
                }
                else
                {
                    LogMessage($"Grid size input changed to: {result:F4}");
                    SetPositionStepSize(result);
                }
            });

            gridRotateSizeInputField.onEndEdit.AddListener(arg0 =>
            {
                if (string.IsNullOrWhiteSpace(arg0) || !float.TryParse(arg0, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                {
                    // Для угла допустимы любые числа, включая 0 и отрицательные, но пустое → ставим по умолчанию
                    float fallback = _rotateStep;
                    LogMessage($"Invalid or empty rotate step input: '{arg0}'. Resetting to {fallback:F4}");
                    gridRotateSizeInputField.text = fallback.ToString(CultureInfo.InvariantCulture);
                    _rotateStep = fallback;
                }
                else
                {
                    LogMessage($"Rotate step input changed to: {result:F4}");
                    _rotateStep = result;
                }
            });
        }

        private void InitializeLogging()
        {
            // ⭐ Проверка, включено ли логирование
            if (!_enableLogging) return;

            try
            {
                // Создаем полный путь к файлу логов
                _fullLogPath = Path.Combine(Application.persistentDataPath, logFilePath);

                // Создаем директорию, если она не существует
                string directory = Path.GetDirectoryName(_fullLogPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    LogMessage($"Created log directory: {directory}"); // Используем LogMessage для этого сообщения
                }

                // Открываем файл для записи (перезаписываем старый)
                _logWriter = new StreamWriter(_fullLogPath, false); // false = перезапись

                LogMessage("=== GridScene Logging Started ===");
                LogMessage($"Application persistent data path: {Application.persistentDataPath}");
                LogMessage($"Log file path: {_fullLogPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize logging: {ex.Message}");
            }
        }

        private void LogMessage(string message)
        {
            // ⭐ Проверка, включено ли логирование
            if (!_enableLogging) return;

            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logEntry = $"[{timestamp}] {message}";

                Debug.Log(logEntry); // Log to Unity console

                if (_logWriter != null)
                {
                    _logWriter.WriteLine(logEntry);
                    _logWriter.Flush(); // Принудительно записываем в файл
                }
            }
            catch (Exception ex)
            {
                // ⭐ Даже ошибки логирования логируем только если логи включены
                if (_enableLogging)
                {
                     Debug.LogError($"Failed to write log message: {ex.Message}");
                }
            }
        }

        private void LogError(string message)
        {
            // ⭐ Проверка, включено ли логирование
            if (!_enableLogging) return;
            
            string errorMessage = $"[ERROR] {message}";
            LogMessage(errorMessage); // Используем уже обновленный LogMessage
        }

        /// <summary>
        /// Устанавливает текущую позицию объекта. Эта позиция используется для
        /// установки оффсета сетки так, чтобы объект оставался на своей позиции.
        /// </summary>
        /// <param name="position">Мировая позиция выбранного объекта.</param>
        public void SetCurrentObjectPosition(Vector2 position)
        {
            LogMessage(
                $"[SetCurrentObjectPosition START] Input position: {position:F4}, Current _positionStepSize: {_positionStepSize:F4}, Current _gridOffset (before): {_gridOffset:F4}");

            if (_positionStepSize <= 0f)
            {
                LogMessage("[SetCurrentObjectPosition] Position step size is invalid (<= 0). Resetting grid offset.");
                _gridOffset = Vector2.zero;
                LogMessage(
                    $"[SetCurrentObjectPosition END] _gridOffset set to: {_gridOffset:F4} (due to invalid step)");
                return;
            }

            // ⭐ Сохраняем позицию объекта для последующего использования при смене шага
            _lastObjectPosition = position;

            // ⭐⭐⭐ ВАЖНО: Новый оффсет рассчитывается так, чтобы `position` снапилась к себе же.
            // SnapToGrid(position) даст ближайшую точку сетки без оффсета.
            // Разница между реальной позицией и этой точкой и есть нужный оффсет.
            float snappedX = SnapToGrid(position.x);
            float snappedY = SnapToGrid(position.y);
            Vector2 newOffset = position - new Vector2(snappedX, snappedY);
            _gridOffset = newOffset;

            LogMessage(
                $"[SetCurrentObjectPosition END] _gridOffset calculated. Input position: {position:F4} -> snapped to ({snappedX:F4}, {snappedY:F4}). New _gridOffset: {_gridOffset:F4}. Current _positionStepSize: {_positionStepSize:F4}");
        }


        /// <summary>
        /// Устанавливает новый шаг сетки позиций.
        /// Оффсет НЕ пересчитывается автоматически.
        /// </summary>
        /// <param name="newStepSize">Новый размер шага сетки.</param>
        public void SetPositionStepSize(float newStepSize)
        {
            LogMessage($"[SetPositionStepSize START] New step: {newStepSize:F4}, Old step: {_positionStepSize:F4}, Current _gridOffset: {_gridOffset:F4}");

            if (newStepSize <= 0)
            {
                LogMessage("[SetPositionStepSize] New step size is invalid (<= 0). Ignoring.");
                return;
            }

            if (Mathf.Approximately(newStepSize, _positionStepSize))
            {
                LogMessage("[SetPositionStepSize] New step size is approximately equal to current step size. No changes needed.");
                return;
            }

            float oldStepSize = _positionStepSize;
            _positionStepSize = newStepSize;

            // ⭐ ВАЖНО: Оффсет НЕ пересчитываем здесь.
            // Он будет пересчитан в PositionController при получении GridPositionEvent.

            _gameEventBus?.Raise(new GridPositionEvent(newStepSize));
            LogMessage($"[SetPositionStepSize END] _positionStepSize updated to: {newStepSize:F4}. _gridOffset remains: {_gridOffset:F4}. GridPositionEvent raised.");
        }

        // ⭐ Основной метод снаппинга — с поддержкой оффсета и вращения
        public Vector2 PositionFloatSnapToGrid(Vector2 value, Quaternion inverseRotation)
        {
            LogMessage($"[PositionFloatSnapToGrid] Input value: {value:F4}, Inverse Rotation: {inverseRotation.eulerAngles:F4}");

            // ⭐ 1. Переводим мировую позицию в локальную систему координат
            Vector3 localValue = inverseRotation * new Vector3(value.x, value.y, 0);
            Vector2 localValue2D = new Vector2(localValue.x, localValue.y);
            LogMessage($"[PositionFloatSnapToGrid] Local value (after inverse rotation): {localValue2D:F4}");

            // ⭐ 2. Применяем снаппинг с оффсетом в ЛОКАЛЬНОЙ системе
            Vector2 snappedLocal = SnapToGridWithOffset(localValue2D, _positionStepSize, _gridOffset);
            LogMessage($"[PositionFloatSnapToGrid] Snapped local value: {snappedLocal:F4}");

            // ⭐ 3. Переводим обратно в мировую систему
            Vector3 snappedWorld = Quaternion.Inverse(inverseRotation) * new Vector3(snappedLocal.x, snappedLocal.y, 0);
            Vector2 result = new Vector2(snappedWorld.x, snappedWorld.y);
            LogMessage($"[PositionFloatSnapToGrid] Final snapped world value: {result:F4}");

            return result;
        }

        public float RotateSnapToGrid(float value)
        {
            LogMessage($"[RotateSnapToGrid] Input value: {value:F4}, Step: {_rotateStep:F4}");
            float result = Mathf.Round(value / _rotateStep) * _rotateStep;
            LogMessage($"[RotateSnapToGrid] Snapped value: {result:F4}");
            return result;
        }

        // ⭐ ВАЖНО: возвращаем этот метод — он используется другими скриптами!
        public float SnapToGrid(float value)
        {
            LogMessage($"[SnapToGrid] Input value: {value:F4}, Step: {_positionStepSize:F4}");

            if (_positionStepSize <= 0)
            {
                LogMessage("[SnapToGrid] Position step size is invalid. Returning original value.");
                return value;
            }

            float result = Mathf.Round(value / _positionStepSize) * _positionStepSize;
            LogMessage($"[SnapToGrid] Snapped value: {result:F4}");
            return result;
        }

        // ⭐ Вспомогательный метод: снаппинг с оффсетом (только для внутреннего использования)
        private Vector2 SnapToGridWithOffset(Vector2 position, float step, Vector2 offset)
        {
            LogMessage($"[SnapToGridWithOffset] Input position: {position:F4}, Step: {step:F4}, Offset: {offset:F4}");

            if (step <= 0f)
            {
                LogMessage("[SnapToGridWithOffset] Step size is invalid. Returning original position.");
                return position;
            }

            Vector2 adjusted = position - offset;
            LogMessage($"[SnapToGridWithOffset] Adjusted position (position - offset): {adjusted:F4}");

            Vector2 snapped = new Vector2(
                Mathf.Round(adjusted.x / step) * step,
                Mathf.Round(adjusted.y / step) * step
            );
            LogMessage($"[SnapToGridWithOffset] Snapped position (on grid): {snapped:F4}");

            Vector2 result = snapped + offset;
            LogMessage($"[SnapToGridWithOffset] Final result (snapped + offset): {result:F4}");

            return result;
        }

        private void OnDestroy()
        {
            LogMessage("=== GridScene destroyed ===");
            if (_logWriter != null)
            {
                _logWriter.Close();
                _logWriter = null;
            }
        }

        private void OnApplicationQuit()
        {
            LogMessage("=== Application quit ===");
            if (_logWriter != null)
            {
                _logWriter.Close();
                _logWriter = null;
            }
        }

        public string GetLogFilePath()
        {
            return _fullLogPath;
        }

        public void ClearLogFile()
        {
            try
            {
                if (_logWriter != null)
                {
                    _logWriter.Close();
                }

                if (File.Exists(_fullLogPath))
                {
                    File.WriteAllText(_fullLogPath, string.Empty);
                    LogMessage("=== Log file cleared ===");
                }

                _logWriter = new StreamWriter(_fullLogPath, false); // false = перезапись
                LogMessage("=== New logging session started ===");
            }
            catch (Exception ex)
            {
                LogError($"Failed to clear log file: {ex.Message}");
            }
        }
        
        // ⭐ Метод для получения последней позиции объекта (может быть полезен)
        public Vector2 GetLastObjectPosition()
        {
            return _lastObjectPosition;
        }
    }
}