using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.EditroSceneCamera;
using TimeLine.Installers;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace TimeLine
{
    public class SceneCameraController : MonoBehaviour
    {
        [SerializeField] private Camera sceneEditorCamera;
        [SerializeField] private SceneToRawImageConverter sceneToRawImageConverter;

        [Header("Настройки масштаба")] [SerializeField]
        private float zoomSensitivity = 0.1f;

        [SerializeField] private float minSize = 0.1f;
        [SerializeField] private float maxSize = 100f;
        [Space] [SerializeField] private RectTransform rawImage;

        private Vector3 _lastMousePosition;
        private GameEventBus _gameEventBus;

        private MainObjects _mainObjects;
        private bool _isDragging = false;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, MainObjects mainObjects)
        {
            _gameEventBus = gameEventBus;
            _mainObjects = mainObjects;
        }

        private void Update()
        {
            if (GetCursorPosition())
            {
                HandleZoom();
            }

            HandlePan();
        }


        private bool GetCursorPosition()
        {
            // 1. Геометрическая проверка (внутри ли мы вьюпорта)
            bool isInsideRect = RectTransformUtility.RectangleContainsScreenPoint(rawImage,
                UnityEngine.Input.mousePosition, _mainObjects.MainCamera);

            if (!isInsideRect) return false;

            // 2. Проверка на перекрытие другим интерфейсом
            if (EventSystem.current == null) return true;

            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = UnityEngine.Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count > 0)
            {
                // Если самый верхний объект (index 0) — это не наш rawImage 
                // и не какой-то его дочерний элемент, значит, мы под другим окном.
                GameObject topObject = results[0].gameObject;
                if (topObject != rawImage.gameObject && !topObject.transform.IsChildOf(rawImage))
                {
                    return false;
                }
            }

            return true;
        }

        private void HandleZoom()
        {
            float scrollDelta = UnityEngine.Input.mouseScrollDelta.y;
            if (scrollDelta != 0)
            {
                Vector3? worldPointBefore =
                    sceneToRawImageConverter.ScreenPointToWorldScene(UnityEngine.Input.mousePosition, 0);

                if (worldPointBefore.HasValue)
                {
                    // Экспоненциальный зум
                    float zoomFactor = Mathf.Pow(1.2f, -scrollDelta * zoomSensitivity);
                    sceneEditorCamera.orthographicSize = Mathf.Clamp(sceneEditorCamera.orthographicSize * zoomFactor,
                        minSize, maxSize);

                    Vector3? worldPointAfter =
                        sceneToRawImageConverter.ScreenPointToWorldScene(UnityEngine.Input.mousePosition, 0);

                    if (worldPointAfter.HasValue)
                    {
                        Vector3 deltaPosition = worldPointBefore.Value - worldPointAfter.Value;
                        sceneEditorCamera.transform.position += deltaPosition;
                    }

                    _gameEventBus.Raise(new EditorSceneCameraUpdateViewEvent());
                }
            }
        }

        private void HandlePan()
        {
            // 1. Проверяем условия нажатия клавиш
            bool panKeyPressed = UnityEngine.Input.GetMouseButton(2) ||
                                 (UnityEngine.Input.GetKey(KeyCode.LeftAlt) && UnityEngine.Input.GetMouseButton(0));

            // 2. Логика НАЧАЛА перетаскивания
            if (panKeyPressed && !_isDragging)
            {
                // Начинаем только если мышь внутри RectTransform
                if (GetCursorPosition())
                {
                    _isDragging = true;
                }
            }

            // 3. Логика ЗАВЕРШЕНИЯ перетаскивания
            if (!panKeyPressed)
            {
                _isDragging = false;
            }

            // 4. Логика ПРОЦЕССА перетаскивания
            if (_isDragging)
            {
                Vector3? currentMouseWorld =
                    sceneToRawImageConverter.ScreenPointToWorldScene(UnityEngine.Input.mousePosition, 0);
                Vector3? lastMouseWorld = sceneToRawImageConverter.ScreenPointToWorldScene(_lastMousePosition, 0);

                if (currentMouseWorld.HasValue && lastMouseWorld.HasValue)
                {
                    Vector3 delta = lastMouseWorld.Value - currentMouseWorld.Value;
                    sceneEditorCamera.transform.position += new Vector3(delta.x, delta.y, 0);
                }

                _gameEventBus.Raise(new EditorSceneCameraUpdateViewEvent());
            }

            // Обновляем позицию всегда, чтобы не было "прыжка" при входе в окно
            _lastMousePosition = UnityEngine.Input.mousePosition;
        }
    }
}