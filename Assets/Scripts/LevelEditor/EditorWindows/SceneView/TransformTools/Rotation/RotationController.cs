using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.EditroSceneCamera;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class RotationController : MonoBehaviour
    {
        // Вспомогательный класс для хранения данных объектов при вращении
        private class RotationData
        {
            public TransformComponent Transform;
            public Vector2 StartPosition;
            public float StartRotation;
        }

        [SerializeField] private RectTransform tool;
        [SerializeField] private RotateTool rotateTool;
        [FormerlySerializedAs("camera")] [SerializeField] private Camera edit_camera_UI;
        [SerializeField] private RectTransform toolCanvas;

        private Action _toolFollowingObject;
        private GameEventBus _gameEventBus;
        private GridScene _gridScene;
        private SceneToRawImageConverter _sceneToRawImageConverter;
        private CoordinateSystem _coordinateSystem;

        private List<RotationData> _selectedObjects = new List<RotationData>();
        private Vector2 _groupCenter;

        [Inject]
        private void Construct(GameEventBus gameEventBus, GridScene gridScene, SceneToRawImageConverter sceneToRawImageConverter, CoordinateSystem coordinateSystem)
        {
            _gameEventBus = gameEventBus;
            _gridScene = gridScene;
            _sceneToRawImageConverter = sceneToRawImageConverter;
            _coordinateSystem = coordinateSystem;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo(((ref SelectObjectEvent data) => Select(data.Tracks)));
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => Select(data.SelectedObjects));
            _gameEventBus.SubscribeTo((ref EditorSceneCameraUpdateViewEvent data) => UpdataPosition());
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) =>
            {
                _selectedObjects = new List<RotationData>();
            });

            // Логика перемещения инструмента за объектами
            _toolFollowingObject = UpdateToolUI;

            rotateTool.onRotate = (deltaAngle) =>
            {
                if (_selectedObjects.Count == 0) return;

                if (_selectedObjects.Count == 1)
                {
                    // Одиночный объект: просто меняем ZRotation
                    var obj = _selectedObjects[0];
                    obj.Transform.ZRotation.Value = _gridScene.RotateSnapToGrid(obj.StartRotation + deltaAngle);
                }
                else
                {
                    // Групповое вращение
                    RotateGroup(deltaAngle);
                }
            };

            rotateTool.StartRotationAction = () =>
            {
                // Перед началом вращения фиксируем текущие данные и центр
                _groupCenter = GetCenter.GetSelectionCenter(_selectedObjects.Select(x => x.Transform).ToList());
                
                foreach (var item in _selectedObjects)
                {
                    item.StartPosition = new Vector2(item.Transform.XPosition.Value, item.Transform.YPosition.Value);
                    item.StartRotation = item.Transform.ZRotation.Value;
                }
            };

            _coordinateSystem.OnCoordinateChanged += isGlobal =>
            {
                if (isGlobal)
                {
                    var transforms = _selectedObjects.Select(x => x.Transform).ToList();
                    _groupCenter = GetCenter.GetSelectionCenter(transforms);
                    tool.position = _sceneToRawImageConverter.WorldToUIPosition(_groupCenter);
                }
                else
                {
                    tool.position = _sceneToRawImageConverter.WorldToUIPosition(new Vector2(_selectedObjects[^1].Transform.XPosition.Value, _selectedObjects[^1].Transform.YPosition.Value));
                }
            };
        }
        
        private void UpdataPosition()
        {
            if (_selectedObjects.Count == 0) return;

            Vector2 targetWorldPos;

            if (_coordinateSystem.IsGlobal)
            {
                // Позиция в центре всех выбранных объектов
                var transforms = _selectedObjects.Select(x => x.Transform).ToList();
                targetWorldPos = GetCenter.GetSelectionCenter(transforms);
            }
            else
            {
                // Позиция на последнем выбранном объекте
                var lastObj = _selectedObjects[^1].Transform;
                targetWorldPos = new Vector2(lastObj.XPosition.Value, lastObj.YPosition.Value);
            }

            // Конвертация мировых координат в позицию UI
            tool.position = _sceneToRawImageConverter.WorldToUIPosition(targetWorldPos);
    
            // Опционально: если гизмо вращения должно визуально отражать поворот объекта в Local режиме
            if (!_coordinateSystem.IsGlobal)
            {
                tool.rotation = Quaternion.Euler(0, 0, _selectedObjects[^1].Transform.ZRotation.Value);
            }
            else
            {
                tool.rotation = Quaternion.identity;
            }
        }

        private void Select(List<TrackObjectPacket> data)
        {
            // 1. Отписываемся от старых объектов
            foreach (var item in _selectedObjects)
            {
                if(item.Transform.XPosition == null || item.Transform.YPosition == null) continue;
                item.Transform.XPosition.OnValueChanged -= _toolFollowingObject;
                item.Transform.YPosition.OnValueChanged -= _toolFollowingObject;
            }

            // 2. Очищаем список
            _selectedObjects.Clear();

            // 3. Добавляем новые
            foreach (var trackData in data)
            {
                if (trackData.sceneObject.TryGetComponent<TransformComponent>(out var comp))
                {
                    _selectedObjects.Add(new RotationData { Transform = comp });
                }
            }

            if (_selectedObjects.Count == 0)
            {
                tool.gameObject.SetActive(false);
                return;
            }

            // tool.gameObject.SetActive(true);

            // 4. Подписываемся на изменения и обновляем UI
            foreach (var item in _selectedObjects)
            {
                item.Transform.XPosition.OnValueChanged += _toolFollowingObject;
                item.Transform.YPosition.OnValueChanged += _toolFollowingObject;
            }

            UpdateToolUI();
        }

        private void UpdateToolUI()
        {
            if (_selectedObjects.Count == 0) return;

            var transforms = _selectedObjects.Select(x => x.Transform).ToList();
            Vector2 currentCenter = GetCenter.GetSelectionCenter(transforms);
            tool.position = _sceneToRawImageConverter.WorldToUIPosition(currentCenter);
        }

        public void RotateGroup(float deltaAngle)
        {
            // 1. Снапим САМ УГОЛ. 
            // Теперь вся группа будет поворачиваться только на разрешенные углы (например, 0, 45, 90...)
            float snappedDeltaAngle = _gridScene.RotateSnapToGrid(deltaAngle);

            // 2. Создаем кватернион на основе заснапленного угла
            Quaternion rotation = Quaternion.Euler(0, 0, snappedDeltaAngle);

            foreach (var item in _selectedObjects)
            {
                // 3. Вычисляем позицию на основе заснапленного поворота
                // Мы используем StartPosition, поэтому деформации не будет
                Vector3 direction = (Vector3)item.StartPosition - (Vector3)_groupCenter;
                Vector3 rotatedDirection = rotation * direction;

                // 4. Применяем позиции БЕЗ SnapToGrid (потому что снап уже заложен в угле)
                // Если применить здесь SnapToGrid, объекты начнут "дрожать" и съезжаться к центру
                if (_coordinateSystem.IsGlobal)
                {
                    Vector3 newPosition = (Vector3)_groupCenter + rotatedDirection;
                    item.Transform.XPosition.Value = newPosition.x;
                    item.Transform.YPosition.Value = newPosition.y;
                }


                // 5. Поворот самого объекта тоже на заснапленный угол
                item.Transform.ZRotation.Value = item.StartRotation + snappedDeltaAngle;
            }
        }
        
        
        public void EnableTool()
        {
            if (_coordinateSystem.IsGlobal)
            {
                var transforms = _selectedObjects.Select(x => x.Transform).ToList();
                _groupCenter = GetCenter.GetSelectionCenter(transforms);
                tool.position = _sceneToRawImageConverter.WorldToUIPosition(_groupCenter);
            }
            else
            {
                tool.position = _sceneToRawImageConverter.WorldToUIPosition(new Vector2(_selectedObjects[^1].Transform.XPosition.Value, _selectedObjects[^1].Transform.YPosition.Value));
            }
        }

        private void OnDestroy()
        {
            foreach (var item in _selectedObjects)
            {
                if (item.Transform == null) continue;
                item.Transform.XPosition.OnValueChanged -= _toolFollowingObject;
                item.Transform.YPosition.OnValueChanged -= _toolFollowingObject;
            }
        }
    }
}