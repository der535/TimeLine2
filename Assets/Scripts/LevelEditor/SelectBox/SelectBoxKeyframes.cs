using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeTimeLine.KeyframeSelect;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.LevelEditor.SelectBox
{
    public class SelectBoxKeyframes : MonoBehaviour
    {
        [SerializeField] private RectTransform selectBox; // Объект который будет показывать область выделения
        [SerializeField] private RectTransform timeLineArea; //Объект внутри которого будет работать выделение
        [SerializeField] private Camera timeLineCamera;
        [SerializeField] private RectTransform deltaX;
        [SerializeField] private RectTransform deltaY;
        [SerializeField] private float deadZoneDistance = 5f; // Пиксели или единицы канваса

        private GameEventBus _gameEventBus;

        private M_SelectBoxState _state = new();
        private M_SelectBoxDelta _delta = new();

        private KeyfeameVizualizer _keyfeameVizualizer;
        private M_KeyframeSelectedStorage _keyframeSelectedStorage;
        private KeyframeSelectController _keyframeSelectController;


        [Inject]
        private void Constructor(GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage,
            DeselectObject deselectObject, SelectObjectController selectObjectController, KeyfeameVizualizer keyfeameVizualizer, M_KeyframeSelectedStorage selectedStorage, KeyframeSelectController keyframeSelectController)
        {
            _gameEventBus = gameEventBus;
            _keyfeameVizualizer = keyfeameVizualizer;
            _keyframeSelectedStorage = selectedStorage;
            _keyframeSelectController = keyframeSelectController;
        }

        private void Awake()
        {
            _state = new M_SelectBoxState();
            _delta = new M_SelectBoxDelta();
            
            _gameEventBus.SubscribeTo((ref SelectObjectEvent selectBox) => { _state.IsActive = true; });
            _gameEventBus.SubscribeTo((ref SelectKeyframeEvent selectBox) => { _state.IsActive = false; });
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent deselectBox) => {  _state.IsActive = false; });
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent all) => { _state.IsActive = false; });
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

            if(!_state.CursorIsInside) return;

            if (_state.IsDragging)
            {
                Vector2 currentMousePos = TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).position;

                // Проверка мертвой зоны, если она еще не пройдена
                if (!_state.HasExceededDeadZone)
                {
                    if (Vector2.Distance(_state.StartPosition, currentMousePos) > deadZoneDistance)
                    {
                        _state.HasExceededDeadZone = true;
                        selectBox.gameObject.SetActive(true); // Включаем рамку только здесь
                        // _deselectObject.Deselect();
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
            _state.CursorIsInside  = TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).isInside;
            _state.IsDragging = true;
            _state.HasExceededDeadZone = false;
            _state.StartPosition = TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).position;
            _delta.startDelta.x = deltaX.offsetMin.x;
            _delta.startDelta.y = deltaY.anchoredPosition.y;
        }

        private void EndMove()
        {
            selectBox.gameObject.SetActive(false);
            _state.IsDragging = false;
            _state.HasExceededDeadZone = false;
        }

        private void SelectBoxCalculate()
        {
            // --- Остальной ваш код расчета позиции рамки и выбора объектов ---
            float xDelta = deltaX.offsetMin.x - _delta.startDelta.x;
            float yDelta = deltaY.anchoredPosition.y - _delta.startDelta.y;
            Vector2 mouseStartPositionModified =
                new Vector2(_state.StartPosition.x + xDelta, _state.StartPosition.y + yDelta);

            Vector2 delta = TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).position - mouseStartPositionModified;
            selectBox.sizeDelta = new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
            selectBox.anchoredPosition = (mouseStartPositionModified + TimeLineConverter.Instance.GetMousePosition(timeLineArea, timeLineCamera).position) / 2;
        }

        private void UpdateSelection() //Логика подсчёта выделенных объектов
        {
            int couter = 0;

            foreach (var keyfrmae in _keyfeameVizualizer.GetAllKeyframesObjectData())
            {
                if (CheckIsSelected(keyfrmae.RectTransform, selectBox))
                {
                    if (!_keyframeSelectedStorage.Keyframes.Contains(keyfrmae.KeyframeDrag._keyframe))
                    {
                        _keyframeSelectController.SelectKeyframe(keyfrmae.KeyframeDrag._keyframe);
                    }
                }
                else
                {
                    if (_keyframeSelectedStorage.Keyframes.Contains(keyfrmae.KeyframeDrag._keyframe))
                    {
                        _keyframeSelectController.DeselectKeyframe(keyfrmae.KeyframeDrag._keyframe);
                    }
                }
                
            }
        }

        private bool CheckIsSelected(RectTransform target, RectTransform selectionBox)
        {
            // Получаем 4 угла целевого объекта в мировых координатах
            Vector3[] targetCorners = new Vector3[4];
            target.GetWorldCorners(targetCorners);

            // Получаем 4 угла рамки выделения в мировых координатах
            Vector3[] selectionCorners = new Vector3[4];
            selectionBox.GetWorldCorners(selectionCorners);

            // Создаем Bounds (границы) для рамки
            // Минимальный угол [0], Максимальный угол [2]
            Bounds selectionBounds = new Bounds(selectionCorners[0], Vector3.zero);
            selectionBounds.Encapsulate(selectionCorners[2]);

            // Проверяем, попадает ли центр или углы цели в границы рамки
            // Для точности лучше проверить, пересекаются ли Bounds
            Bounds targetBounds = new Bounds(targetCorners[0], Vector3.zero);
            targetBounds.Encapsulate(targetCorners[2]);

            return selectionBounds.Intersects(targetBounds);
        }
    }
}