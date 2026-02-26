using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.SelectBox
{
    public class SelectBoxSceneObjects : MonoBehaviour
    {
        [SerializeField] private RectTransform selectBox; // Объект который будет показывать область выделения
        [SerializeField] private RectTransform timeLineArea; //Объект внутри которого будет работать выделение
        [SerializeField] private Camera timeLineCamera;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private float deadZoneDistance = 5f; // Пиксели или единицы канваса

        private GameEventBus _gameEventBus;
        private DeselectObject _deselectObject;
        private SelectObjectController _selectObjectController;
        private SceneToRawImageConverter _sceneToRawImageConverter;
        private TrackObjectStorage _trackObjectStorage;

        private M_SelectBoxState _state = new();
        private M_SelectBoxDelta _delta = new();


        [Inject]
        private void Constructor(GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage,
            DeselectObject deselectObject, SelectObjectController selectObjectController,
            SceneToRawImageConverter sceneToRawImageConverter)
        {
            _trackObjectStorage = trackObjectStorage;
            _gameEventBus = gameEventBus;
            _deselectObject = deselectObject;
            _selectObjectController = selectObjectController;
            _sceneToRawImageConverter = sceneToRawImageConverter;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref SelectObjectEvent selectBox) => { _state.IsActive = false; });
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent deselectBox) => { _state.IsActive = true; });
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent all) => { _state.IsActive = true; });
        }

        private void Update()
        {
            if (_state.IsActive == false && _state.IsDragging == false)
            {
                return;
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                StartMove();
            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                EndMove();
            }


            if (_state.IsDragging && _state.CursorIsInside)
            {
                Vector2 currentMousePos =
                    TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).position;

                // Проверка мертвой зоны, если она еще не пройдена
                if (!_state.HasExceededDeadZone)
                {
                    if (Vector2.Distance(_state.StartPosition, currentMousePos) > deadZoneDistance)
                    {
                        _state.HasExceededDeadZone = true;
                        selectBox.gameObject.SetActive(true); // Включаем рамку только здесь
                        _deselectObject.Deselect();
                    }
                    else
                    {
                        return; // Выходим из Update, пока мы внутри мертвой зоны
                    }
                }

                SelectBoxCalculate();
                // Оптимизация: выборку объектов лучше делать только если рамка активна
                UpdateSelection();
            }
        }

        private Vector2 startPositionOnScene;

        private void StartMove()
        {
            // Только инициализируем данные, но рамку пока не включаем
            _state.CursorIsInside = TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).isInside;

            _state.IsDragging = true;
            _state.HasExceededDeadZone = false;
            _state.StartPosition = TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).position;
            _delta.startDelta = _sceneToRawImageConverter.ScreenPointToWorldScene(_state.StartPosition);

            startPositionOnScene = _sceneToRawImageConverter.ScreenPointToWorldScene(UnityEngine.Input.mousePosition);
        }

        private void EndMove()
        {
            selectBox.gameObject.SetActive(false);
            _state.IsDragging = false;
            _state.HasExceededDeadZone = false;
        }

        private void SelectBoxCalculate()
        {
            Vector2 mouseStartPositionModified =
                _sceneToRawImageConverter.WorldToUIAnchoredPosition(startPositionOnScene, timeLineArea);

            Vector2 delta = TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).position -
                            mouseStartPositionModified;
            selectBox.sizeDelta = new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
            selectBox.anchoredPosition = (mouseStartPositionModified +
                                          TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera)
                                              .position) / 2;
        }

        private void UpdateSelection() //Логика подсчёта выделенных объектов
        {
            Check(_trackObjectStorage.GetAllActiveSceneObjects());
        }

        private void Check(List<TrackObjectPacket> list, TrackObjectPacket parentGroup = null)
        {
            foreach (var data in list)
            {
                if (data is TrackObjectGroup group)
                {
                    Check(group.TrackObjectDatas, parentGroup ?? group);
                }
                else
                {
                    var targetObject = parentGroup ?? data;
                    if (CheckIsSelected(data.sceneObject, selectBox))
                    {
                        if (!_selectObjectController.SelectObjects.Contains(targetObject))
                        {
                            // print("group S");

                            _selectObjectController.SelectNoClear(targetObject);
                            if(parentGroup != null) break;
                        }
                    }
                    else
                    {
                        if (_selectObjectController.SelectObjects.Contains(targetObject))
                        {
                            // print("group D");
                            _selectObjectController.Deselect(targetObject);
                        }
                    }
                }
            }
        }

        private bool CheckIsSelected(GameObject target, RectTransform selectionBox)
        {
            // 1. Получаем Renderer объекта (SpriteRenderer для 2D)
            if (!target.TryGetComponent<SpriteRenderer>(out var renderer))
            {
                print(false);
                return false;
            }

            Renderer targetRenderer = renderer;

            // 2. Получаем границы объекта в мировом 3D/2D пространстве
            Bounds worldBounds = targetRenderer.bounds;

            // 3. Находим крайние точки (Min и Max) в координатах UI
            // Мы берем 8 углов для 3D или 4 угла для 2D, чтобы точно вычислить проекцию
            Vector3 minWorld = worldBounds.min;
            Vector3 maxWorld = worldBounds.max;

            // Углы прямоугольника объекта в 2D мире
            Vector3[] worldCorners = new Vector3[]
            {
                new Vector3(minWorld.x, minWorld.y, minWorld.z),
                new Vector3(minWorld.x, maxWorld.y, minWorld.z),
                new Vector3(maxWorld.x, minWorld.y, minWorld.z),
                new Vector3(maxWorld.x, maxWorld.y, minWorld.z)
            };

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            // Проецируем каждый угол в UI пространство timeLineArea
            foreach (var corner in worldCorners)
            {
                Vector2 uiPos = _sceneToRawImageConverter.WorldToUIAnchoredPosition(corner, timeLineArea);
                if (uiPos.x < minX) minX = uiPos.x;
                if (uiPos.x > maxX) maxX = uiPos.x;
                if (uiPos.y < minY) minY = uiPos.y;
                if (uiPos.y > maxY) maxY = uiPos.y;
            }

            // Создаем Rect, описывающий объект в пространстве UI
            Rect objectRectInUI = new Rect(minX, minY, maxX - minX, maxY - minY);

            // 4. Получаем Rect самой рамки выделения
            Rect selectionRect = selectionBox.rect;
            selectionRect.x += selectionBox.anchoredPosition.x;
            selectionRect.y += selectionBox.anchoredPosition.y;

            // 5. Проверяем пересечение двух прямоугольников (Overlaps)
            // Это вернет true, если рамка хотя бы краем задела объект
            // print(selectionRect.Overlaps(objectRectInUI));
            return selectionRect.Overlaps(objectRectInUI);
        }


// Вспомогательный метод для создания границ рамки в координатах Viewport
        private Bounds GetViewportBounds(RectTransform selectionBox, Camera cam)
        {
            Vector3[] corners = new Vector3[4];
            selectionBox.GetWorldCorners(corners);

            Bounds bounds = new Bounds();
            for (int i = 0; i < 4; i++)
            {
                // Переводим углы UI рамки в координаты Viewport камеры
                Vector3 viewportCorner = cam.ScreenToViewportPoint(corners[i]);
                if (i == 0) bounds.SetMinMax(viewportCorner, viewportCorner);
                else bounds.Encapsulate(viewportCorner);
            }

            // Обнуляем Z, так как нам важна только плоскость экрана
            Vector3 min = bounds.min;
            min.z = 0f;
            Vector3 max = bounds.max;
            max.z = 1f; // Даем небольшой запас по глубине
            bounds.SetMinMax(min, max);

            return bounds;
        }
    }
}