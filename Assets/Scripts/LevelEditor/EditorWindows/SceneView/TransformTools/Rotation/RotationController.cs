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

        private GameEventBus _gameEventBus;
        private GridScene _gridScene;
        private SceneToRawImageConverter _sceneToRawImageConverter;
        private CoordinateSystem _coordinateSystem;

        private List<RotationToolData> _selectedObjects = new();
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

            rotateTool.onRotate = (deltaAngle) =>
            {
                if (_selectedObjects.Count == 0) return;

                if (_selectedObjects.Count == 1)
                {
                    var obj = _selectedObjects[0];
                    LocalTransform localTransform = _entityManager.GetComponentData<LocalTransform>(obj.Entity);
                    RotationData rotationData = _entityManager.GetComponentData<RotationData>(obj.Entity);

                    float newZ = _gridScene.RotateSnapToGrid(obj.StartRotation + deltaAngle);
        
                    // ЛОГ 1: Что мы насчитали

                    rotationData.RotateZ = newZ;
                    localTransform.Rotation = GetDegree.FromEuler(new Vector3(0, 0, rotationData.RotateZ));

                    _entityManager.SetComponentData(obj.Entity, localTransform);
                    _entityManager.SetComponentData(obj.Entity, rotationData);
                }
                else
                {
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
                    item.StartRotation = rotationData.RotateZ; // Тут теперь честное значение
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
                var rotData = _entityManager.GetComponentData<RotationData>(_selectedObjects[^1].Entity);
                tool.rotation = Quaternion.Euler(0, 0, rotData.RotateZ);
            }
            else
            {
                tool.rotation = Quaternion.identity;
            }
        }

        private void Select(List<TrackObjectPacket> data)
        {
            Debug.Log("Select");
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
            float snappedDeltaAngle = _gridScene.RotateSnapToGrid(deltaAngle);
            Quaternion rotation = Quaternion.Euler(0, 0, snappedDeltaAngle);

            foreach (var item in _selectedObjects)
            {
                LocalTransform localTransform = _entityManager.GetComponentData<LocalTransform>(item.Entity);
                RotationData rotationData = _entityManager.GetComponentData<RotationData>(item.Entity);

                // --- 1. Позиция (Global) ---
                if (_coordinateSystem.IsGlobal)
                {
                    Vector3 direction = (Vector3)item.StartPosition - (Vector3)_groupCenter;
                    Vector3 rotatedDirection = rotation * direction;
                    Vector3 newPosition = (Vector3)_groupCenter + rotatedDirection;

                    localTransform.Position.x = newPosition.x;
                    localTransform.Position.y = newPosition.y;

                    _entityManager.SetComponentData(item.Entity, new PositionData { 
                        Position = new float2(newPosition.x, newPosition.y) 
                    });
                }

                // --- 2. Вращение (Критически важно!) ---
                // ИСПОЛЬЗУЕМ ТОЛЬКО StartRotation. Никаких FromQuaternion!
                float newZ = item.StartRotation + snappedDeltaAngle;
        
                rotationData.RotateZ = newZ;
                // Передаем чистый Z в FromEuler. X и Y обычно 0 для 2D, 
                // если нет - используйте значения из RotationData, но не из трансформа.
                localTransform.Rotation = GetDegree.FromEuler(new Vector3(0, 0, rotationData.RotateZ));

                _entityManager.SetComponentData(item.Entity, rotationData);
                _entityManager.SetComponentData(item.Entity, localTransform);
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