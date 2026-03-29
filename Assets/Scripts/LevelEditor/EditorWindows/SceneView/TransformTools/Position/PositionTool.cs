using System;
using System.IO;
using System.Text;
using TimeLine.Installers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Zenject;
using System.Linq;

namespace TimeLine
{
    public class PositionTool : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [FormerlySerializedAs("_toolTransform")] [SerializeField] private RectTransform toolTransform;

        // 🔽 Укажи путь в Инспекторе, например: "C:/Temp/PositionTool_Log.txt"
        [SerializeField] private string _logPath = "C:/Temp/PositionTool_Log.txt";
        [SerializeField] private bool _enableLogging = true; // ⭐ Галочка для включения/выключения логов

        private MainObjects _mainObjects;
        private Canvas _canvas;

        private bool _isMovingY;
        private bool _isMovingX;
        private bool _isFreeMoving;

        private bool _yNeedsInit = false;
        private bool _xNeedsInit = false;
        private bool _freeNeedsInit = false;

        private Vector2 _mouseOffset;
        private Vector2 _toolStartPosition;
        private Quaternion _inverseRotation;

        public Action<RectTransform> OnChangePosition;
        public Action OnDragEndX; // ⭐ НОВОЕ: событие завершения перетаскивания
        public Action OnDragEndY; // ⭐ НОВОЕ: событие завершения перетаскивания

        public bool isGlobal;

        private Vector2 _lastPosition;
        private Vector2 _lastMousePosition;

        private StreamWriter _logWriter;

        [Inject]
        private void Construct(MainObjects mainObjects)
        {
            _mainObjects = mainObjects;
        }

        private void Awake()
        {
            _canvas = toolTransform.GetComponentInParent<Canvas>();
            _lastPosition = toolTransform.anchoredPosition;
            _lastMousePosition = GetMousePositionInParentSpace();

            // ⭐ Опционально: обернуть инициализацию логгера
            if (_enableLogging)
            {
                InitializeLogger();
            }
            Log("=== PositionTool Session Started ===");
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

                _logWriter = new StreamWriter(_logPath, false, Encoding.UTF8);
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
            _logWriter?.WriteLine(logLine);
            _logWriter?.Flush();
        }

        private bool IsApproximatelyEqual(Vector2 a, Vector2 b, float tolerance = 0.001f)
        {
            // ⭐ Эта вспомогательная функция не требует логирования
            return Mathf.Abs(a.x - b.x) < tolerance && Mathf.Abs(a.y - b.y) < tolerance;
        }

        public void SetMoveY(bool isMovingY)
        {
            if (!isMovingY && _isMovingY)
            {
                OnDragEndY?.Invoke(); // Вызываем при отпускании
                Log("SetMoveY(false) called → OnDragEnd invoked");
            }
            _isMovingY = isMovingY;
            if (isMovingY) _yNeedsInit = true;
            Log($"SetMoveY({isMovingY}) called");
        }

        public void SetMoveX(bool isMovingX)
        {
            if (!isMovingX && _isMovingX)
            {
                OnDragEndX?.Invoke();
                Log("SetMoveX(false) called → OnDragEnd invoked");
            }
            _isMovingX = isMovingX;
            if (isMovingX) _xNeedsInit = true;
            Log($"SetMoveX({isMovingX}) called");
        }

        public void SetFreeMove(bool isFreeMoving)
        {
            if (!isFreeMoving && _isFreeMoving)
            {
                OnDragEndY?.Invoke();
                OnDragEndX?.Invoke();
                Log("SetFreeMove(false) called → OnDragEnd invoked");
            }
            _isFreeMoving = isFreeMoving;
            if (isFreeMoving) _freeNeedsInit = true;
            Log($"SetFreeMove({isFreeMoving}) called");
        }

        private Vector2 GetMousePositionInParentSpace()
        {
            Vector2 mousePos = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            RectTransform parentRect = toolTransform.parent as RectTransform;
            if (parentRect == null)
            {
                Log("WARNING: toolTransform.parent is not RectTransform!");
                return Vector2.zero;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    mousePos,
                    _canvas?.worldCamera,
                    out var localPosition))
            {
                Log("WARNING: RectTransformUtility failed to convert mouse position!");
                return Vector2.zero;
            }

            return localPosition;
        }

        private void SaveMouseOffset()
        {
            _mouseOffset = GetMousePositionInParentSpace();
            _toolStartPosition = toolTransform.anchoredPosition;

            float rotationAngle = -toolTransform.localEulerAngles.z;
            _inverseRotation = Quaternion.Euler(0, 0, rotationAngle);

            _lastMousePosition = _mouseOffset;

            Log($"[INIT] MouseOffset: {_mouseOffset:F4}, ToolStart: {_toolStartPosition:F4}, Rotation: {toolTransform.localEulerAngles.z:F2}");
        }

        private void Update()
        {
            Vector2 currentMousePos = GetMousePositionInParentSpace();

            // Проверка на внешнее изменение позиции
            if (!IsApproximatelyEqual(toolTransform.anchoredPosition, _lastPosition))
            {
                Vector2 externalChange = toolTransform.anchoredPosition;
                Log($"[EXTERNAL CHANGE DETECTED] Position changed from {_lastPosition:F4} to {externalChange:F4} outside Move logic!");
                _lastPosition = externalChange;
            }

            // Инициализация при первом реальном движении мыши
            if (_yNeedsInit && !IsApproximatelyEqual(currentMousePos, _lastMousePosition, 0.1f))
            {
                SaveMouseOffset();
                _yNeedsInit = false;
                Log("Y movement initialized due to mouse movement.");
            }
            if (_xNeedsInit && !IsApproximatelyEqual(currentMousePos, _lastMousePosition, 0.1f))
            {
                SaveMouseOffset();
                _xNeedsInit = false;
                Log("X movement initialized due to mouse movement.");
            }
            if (_freeNeedsInit && !IsApproximatelyEqual(currentMousePos, _lastMousePosition, 0.1f))
            {
                SaveMouseOffset();
                _freeNeedsInit = false;
                Log("Free movement initialized due to mouse movement.");
            }

            _lastMousePosition = currentMousePos;

            MoveY();
            MoveX();
            FreeMove();
        }

        private void MoveY()
        {
            if (!_isMovingY || _yNeedsInit) return;

            Vector2 currentMousePos = GetMousePositionInParentSpace();
            Vector2 delta = currentMousePos - _mouseOffset;

            Vector2 newPosition;
            if (!isGlobal)
            {
                Vector2 localDelta = _inverseRotation * delta;
                Vector2 movement = new Vector2(0, localDelta.y);
                Vector2 globalMovement = Quaternion.Inverse(_inverseRotation) * movement;
                newPosition = _toolStartPosition + globalMovement;
            }
            else
            {
                newPosition = _toolStartPosition + new Vector2(0, delta.y);
            }

            toolTransform.anchoredPosition = newPosition;

            Log($"[MOVE Y] Delta: {delta:F4}, NewPos: {newPosition:F4}, AnchoredPos: {toolTransform.anchoredPosition:F4}");

            if (!IsApproximatelyEqual(newPosition, _lastPosition))
            {
                _lastPosition = newPosition;
                OnChangePosition?.Invoke(toolTransform);
                Log($"[EVENT] OnChangePosition invoked with: {newPosition:F4}");
            }
        }

        private void MoveX()
        {
            if (!_isMovingX || _xNeedsInit) return;

            Vector2 currentMousePos = GetMousePositionInParentSpace();
            Vector2 delta = currentMousePos - _mouseOffset;

            Vector2 newPosition;
            if (!isGlobal)
            {
                Vector2 localDelta = _inverseRotation * delta;
                Vector2 movement = new Vector2(localDelta.x, 0);
                Vector2 globalMovement = Quaternion.Inverse(_inverseRotation) * movement;
                newPosition = _toolStartPosition + globalMovement;
            }
            else
            {
                newPosition = _toolStartPosition + new Vector2(delta.x, 0);
            }

            toolTransform.anchoredPosition = newPosition;

            Log($"[MOVE X] Delta: {delta:F4}, NewPos: {newPosition:F4}, AnchoredPos: {toolTransform.anchoredPosition:F4}");

            if (!IsApproximatelyEqual(newPosition, _lastPosition))
            {
                _lastPosition = newPosition;
                OnChangePosition?.Invoke(toolTransform);
                Log($"[EVENT] OnChangePosition invoked with: {newPosition:F4}");
            }
        }

        private void FreeMove()
        {
            if (!_isFreeMoving || _freeNeedsInit) return;

            Vector2 currentMousePos = GetMousePositionInParentSpace();
            Vector2 delta = currentMousePos - _mouseOffset;
            Vector2 newPosition = _toolStartPosition + delta;

            toolTransform.anchoredPosition = newPosition;

            Log($"[FREE MOVE] Delta: {delta:F4}, NewPos: {newPosition:F4}, AnchoredPos: {toolTransform.anchoredPosition:F4}");

            if (!IsApproximatelyEqual(newPosition, _lastPosition))
            {
                _lastPosition = newPosition;
                OnChangePosition?.Invoke(toolTransform);
                Log($"[EVENT] OnChangePosition invoked with: {newPosition:F4}");
            }
        }

        private void OnDestroy()
        {
            Log("=== PositionTool Session Ended ===");
            _logWriter?.Close();
            _logWriter?.Dispose();
        }
    }
}