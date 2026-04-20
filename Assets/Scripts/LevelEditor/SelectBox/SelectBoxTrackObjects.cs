using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.Misc;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.Core;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.SelectBox
{
    public class SelectBoxTrackObjects : MonoBehaviour
    {
        [SerializeField] private RectTransform selectBox; // Объект который будет показывать область выделения
        [SerializeField] private RectTransform timeLineArea; //Объект внутри которого будет работать выделение
        [SerializeField] private Camera timeLineCamera;
        [SerializeField] private RectTransform timeMarkerContent;
        [SerializeField] private RectTransform trackObjectsContent;
        [SerializeField] private float deadZoneDistance = 5f; // Пиксели или единицы канваса

        private GameEventBus _gameEventBus;
        private TrackObjectStorage _trackObjectStorage;
        private DeselectObject _deselectObject;
        private SelectObjectController _selectObjectController;

        private M_SelectBoxState _state = new();
        private M_SelectBoxDelta _delta = new();

        private List<TrackObjectPacket> allObjects;
        private List<TrackObjectPacket> selectedObjects = new List<TrackObjectPacket>();

        [Inject]
        private void Constructor(GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage,
            DeselectObject deselectObject, SelectObjectController selectObjectController)
        {
            _gameEventBus = gameEventBus;
            _trackObjectStorage = trackObjectStorage;
            _deselectObject = deselectObject;
            _selectObjectController = selectObjectController;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref SelectObjectEvent _) => { _state.IsActive = false; });
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent _) => { _state.IsActive = true; });
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent _) => { _state.IsActive = true; });
            _gameEventBus.SubscribeTo((ref TrackObjectResizing all) => { _state.IsActive = all.IsResizing == false && _state.IsActive; });
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
                Vector2 currentMousePos = TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).position;

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

        private void StartMove()
        {
            // Только инициализируем данные, но рамку пока не включаем
            _state.CursorIsInside = TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).isInside;

            _state.IsDragging = true;
            _state.HasExceededDeadZone = false;
            _state.StartPosition = TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).position;
            _delta.startDelta.x = timeMarkerContent.offsetMin.x;
            _delta.startDelta.y = trackObjectsContent.anchoredPosition.y;
        }

        private void EndMove()
        {
            selectBox.gameObject.SetActive(false);
            _state.IsDragging = false;
            _state.HasExceededDeadZone = false;
            _selectObjectController.SelectMultiple(selectedObjects);
            _selectObjectController.UpdateSelection();
        }

        private void SelectBoxCalculate()
        {
            // --- Остальной ваш код расчета позиции рамки и выбора объектов ---
            float xDelta = timeMarkerContent.offsetMin.x - _delta.startDelta.x;
            float yDelta = trackObjectsContent.anchoredPosition.y - _delta.startDelta.y;
            Vector2 mouseStartPositionModified =
                new Vector2(_state.StartPosition.x + xDelta, _state.StartPosition.y + yDelta);

            Vector2 delta = TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).position - mouseStartPositionModified;
            selectBox.sizeDelta = new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
            selectBox.anchoredPosition = (mouseStartPositionModified + TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).position) / 2;
        }

        private void UpdateSelection()
        {
            selectedObjects.Clear();
            // Кэшируем данные, чтобы не обращаться к свойствам в каждой итерации
            var currentSelection = _selectObjectController.SelectObjectsHash;
            
            // Если selectBox вычисляется динамически, лучше сохранить его в переменную
            var box = selectBox;

            allObjects = _trackObjectStorage.GetAllActiveTrackData();

            
            // Получаем 4 угла рамки выделения в мировых координатах
            box.GetWorldCorners(_selectionCorners);

            foreach (var trackObject in allObjects)
            {
                bool isInside = CheckIsSelected(trackObject.components.View.GetRectTransform(), box);
                bool isAlreadySelected = currentSelection.Contains(trackObject);


                if (isInside && !isAlreadySelected)
                {
                    selectedObjects.Add(trackObject);
                    // trackObject.components.View.SetColor(Color.yellow);
                    _selectObjectController.SelectNoClearNoEvent(trackObject);
                }
                else if (!isInside && isAlreadySelected)
                {
                    selectedObjects.Remove(trackObject);
                    _selectObjectController.DeselectVihoutEvent(trackObject);
                }
            }
        }

        private readonly Vector3[] _targetCorners = new Vector3[4];
        private readonly Vector3[] _selectionCorners = new Vector3[4];

        private bool CheckIsSelected(RectTransform target, RectTransform selectionBox)
        {
            // Получаем 4 угла целевого объекта в мировых координатах
            target.GetWorldCorners(_targetCorners);


            // Создаем Bounds (границы) для рамки
            // Минимальный угол [0], Максимальный угол [2]
            Bounds selectionBounds = new Bounds(_selectionCorners[0], Vector3.zero);
            selectionBounds.Encapsulate(_selectionCorners[2]);

            // Проверяем, попадает ли центр или углы цели в границы рамки
            // Для точности лучше проверить, пересекаются ли Bounds
            Bounds targetBounds = new Bounds(_targetCorners[0], Vector3.zero);
            targetBounds.Encapsulate(_targetCorners[2]);

            return selectionBounds.Intersects(targetBounds);
        }
    }
}