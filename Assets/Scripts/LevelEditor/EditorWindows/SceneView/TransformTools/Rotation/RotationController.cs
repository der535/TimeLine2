using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.EditroSceneCamera;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.ECS;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class RotationController : MonoBehaviour
    {
        // Вспомогательный класс для хранения данных объектов при вращении
        private class RotationToolData
        {
            public Entity Entity;
            public Vector2 StartPosition;
            public float StartRotation;
        }

        [SerializeField] private RectTransform tool;
        [SerializeField] private RotateTool rotateTool;

        [FormerlySerializedAs("camera")] [SerializeField]
        private Camera edit_camera_UI;

        [SerializeField] private RectTransform toolCanvas;

        private Action _toolFollowingObject;
        private GameEventBus _gameEventBus;
        private GridScene _gridScene;
        private SceneToRawImageConverter _sceneToRawImageConverter;
        private CoordinateSystem _coordinateSystem;

        private List<RotationToolData> _selectedObjects = new List<RotationToolData>();
        private Vector2 _groupCenter;
        private EntityManager _entityManager;

        public Action OnValueChanged;
        public Action OnStopRotation;

        [Inject]
        private void Construct(GameEventBus gameEventBus, GridScene gridScene,
            SceneToRawImageConverter sceneToRawImageConverter, CoordinateSystem coordinateSystem)
        {
            _gameEventBus = gameEventBus;
            _gridScene = gridScene;
            _sceneToRawImageConverter = sceneToRawImageConverter;
            _coordinateSystem = coordinateSystem;
        }

        private void Awake()
        {
            rotateTool.StopRotationAction += () => OnStopRotation.Invoke();
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            _gameEventBus.SubscribeTo(((ref SelectObjectEvent data) => Select(data.Tracks)));
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => Select(data.SelectedObjects));
            _gameEventBus.SubscribeTo((ref EditorSceneCameraUpdateViewEvent data) => UpdataPosition());
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) =>
            {
                _selectedObjects = new List<RotationToolData>();
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
                    LocalTransform localTransform = _entityManager.GetComponentData<LocalTransform>(obj.Entity);
                    RotationData rotationData = _entityManager.GetComponentData<RotationData>(obj.Entity);


                    float newZ = _gridScene.RotateSnapToGrid(obj.StartRotation + deltaAngle);
                    rotationData.RotateZ = newZ;

                    var objectRotation = GetDegree.FromQuaternion(localTransform.Rotation);


                    localTransform.Rotation =
                        GetDegree.FromEuler(new Vector3(objectRotation.x, objectRotation.y, rotationData.RotateZ));

                    _entityManager.SetComponentData(obj.Entity, localTransform);
                    _entityManager.SetComponentData(obj.Entity, rotationData);
                }
                else
                {
                    // Групповое вращение
                    RotateGroup(deltaAngle);
                }

                OnValueChanged.Invoke();
            };

            rotateTool.StartRotationAction = () =>
            {
                // Перед началом вращения фиксируем текущие данные и центр
                _groupCenter = GetCenter.GetSelectionCenter(_selectedObjects
                    .Select(x => _entityManager.GetComponentData<LocalTransform>(x.Entity)).ToList());

                foreach (var item in _selectedObjects)
                {
                    LocalTransform localTransform = _entityManager.GetComponentData<LocalTransform>(item.Entity);
                    RotationData rotationData = _entityManager.GetComponentData<RotationData>(item.Entity);
                    item.StartPosition = new Vector2(localTransform.Position.x, localTransform.Position.y);
                    item.StartRotation = rotationData.RotateZ;
                }
            };

            _coordinateSystem.OnCoordinateChanged += isGlobal =>
            {
                if (isGlobal)
                {
                    var transforms = _selectedObjects
                        .Select(x => _entityManager.GetComponentData<LocalTransform>(x.Entity)).ToList();
                    _groupCenter = GetCenter.GetSelectionCenter(transforms);
                    tool.position = _sceneToRawImageConverter.WorldToUIPosition(_groupCenter);
                }
                else
                {
                    tool.position = _sceneToRawImageConverter.WorldToUIPosition(new Vector2(
                        _entityManager.GetComponentData<LocalTransform>(_selectedObjects[^1].Entity).Position.x,
                        _entityManager.GetComponentData<LocalTransform>(_selectedObjects[^1].Entity).Position.y));
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
                var transforms = _selectedObjects.Select(x => _entityManager.GetComponentData<LocalTransform>(x.Entity))
                    .ToList();
                targetWorldPos = GetCenter.GetSelectionCenter(transforms);
            }
            else
            {
                // Позиция на последнем выбранном объекте
                var lastObj = _entityManager.GetComponentData<LocalTransform>(_selectedObjects[^1].Entity).Position;
                targetWorldPos = new Vector2(lastObj.x, lastObj.y);
            }

            // Конвертация мировых координат в позицию UI
            tool.position = _sceneToRawImageConverter.WorldToUIPosition(targetWorldPos);

            // Опционально: если гизмо вращения должно визуально отражать поворот объекта в Local режиме
            if (!_coordinateSystem.IsGlobal)
            {
                tool.rotation = Quaternion.Euler(0, 0,
                    GetDegree.FromQuaternion(_entityManager
                        .GetComponentData<LocalTransform>(_selectedObjects[^1].Entity).Rotation.value).z);
            }
            else
            {
                tool.rotation = Quaternion.identity;
            }
        }

        private void Select(List<TrackObjectPacket> data)
        {
            // 2. Очищаем список
            _selectedObjects.Clear();

            // 3. Добавляем новые
            foreach (var trackData in data)
            {
                _selectedObjects.Add(new RotationToolData { Entity = trackData.entity });
            }

            if (_selectedObjects.Count == 0)
            {
                tool.gameObject.SetActive(false);
                return;
            }

            UpdateToolUI();
        }

        private void UpdateToolUI()
        {
            if (_selectedObjects.Count == 0) return;

            var transforms = _selectedObjects.Select(x => _entityManager.GetComponentData<LocalTransform>(x.Entity))
                .ToList();
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
                    LocalTransform localTransform = _entityManager.GetComponentData<LocalTransform>(item.Entity);
                    localTransform.Position.x = newPosition.x;
                    localTransform.Position.y = newPosition.y;

                    PositionData positionData = new PositionData();
                    positionData.Position = new float2(localTransform.Position.x, localTransform.Position.y);
                    _entityManager.SetComponentData(item.Entity, positionData);

                    _entityManager.SetComponentData(item.Entity, localTransform);
                }


                var obj = item;
                LocalTransform localTransform2 = _entityManager.GetComponentData<LocalTransform>(obj.Entity);
                Vector3 currentEuler = GetDegree.FromQuaternion(localTransform2.Rotation);
                float newZ = item.StartRotation + snappedDeltaAngle;
                currentEuler.z = newZ;
                localTransform2.Rotation = GetDegree.FromEuler(currentEuler);
                _entityManager.SetComponentData(obj.Entity, localTransform2);
            }
        }


        public void EnableTool()
        {
            if (_coordinateSystem.IsGlobal)
            {
                var transforms = _selectedObjects.Select(x => _entityManager.GetComponentData<LocalTransform>(x.Entity))
                    .ToList();
                _groupCenter = GetCenter.GetSelectionCenter(transforms);
                tool.position = _sceneToRawImageConverter.WorldToUIPosition(_groupCenter);
            }
            else
            {
                LocalTransform localTransform =
                    _entityManager.GetComponentData<LocalTransform>(_selectedObjects[^1].Entity);
                tool.position = _sceneToRawImageConverter.WorldToUIPosition(new Vector2(
                    localTransform.Position.x, localTransform.Position.y));
            }
        }
    }
}