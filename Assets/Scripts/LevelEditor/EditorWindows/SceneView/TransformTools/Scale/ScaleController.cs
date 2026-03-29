using System;
using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using System.Linq;
using TimeLine.EventBus.Events.EditroSceneCamera;
using TimeLine.LevelEditor.ECS;
using TimeLine.LevelEditor.ECS.Services;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace TimeLine
{
    public class ScaleController : MonoBehaviour
    {
        [FormerlySerializedAs("_gridScene")] [SerializeField]
        private GridScene gridScene;

        [FormerlySerializedAs("edit_camera_UI")] [FormerlySerializedAs("camera")] [SerializeField]
        private Camera editCameraUI;

        [SerializeField] private RectTransform tool;

        [FormerlySerializedAs("_scaleTool")] [SerializeField]
        private ScaleTool scaleTool;

        private GameEventBus _eventBus;
        private SceneToRawImageConverter _sceneToRawImageConverter;
        private CoordinateSystem _coordinateSystem;

        private List<(Entity entity, Vector2 startScale, Vector2 startPosition)> _transformComponent = new();

        private Action _objectFollowingTool;

        private Action<float> _horizontalScale;
        private Action<float> _verticalScale;

        private Vector2 _center;

        private EntityManager _entityManager;

        public Action OnValueChanged;
        public Action OnStopScaleX;
        public Action OnStopScaleY;

        [Inject]
        private void Construct(GameEventBus gameEventBus,
            SceneToRawImageConverter sceneToRawImageConverter, CoordinateSystem coordinateSystem)
        {
            _eventBus = gameEventBus;
            _sceneToRawImageConverter = sceneToRawImageConverter;
            _coordinateSystem = coordinateSystem;
        }

        private void Awake()
        {
            scaleTool.OnStopX += () => OnStopScaleX?.Invoke(); 
            scaleTool.OnStopY += () => OnStopScaleY?.Invoke(); 
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            _eventBus.SubscribeTo(((ref SelectObjectEvent data) => { SetPosition(data.Tracks); }));
            _eventBus.SubscribeTo((ref DeselectObjectEvent data) => { SetPosition(data.SelectedObjects); });
            _eventBus.SubscribeTo((ref EditorSceneCameraUpdateViewEvent data) => { UpdatePosition(); });
            _eventBus.SubscribeTo((ref DeselectAllObjectEvent data) =>
            {
                _transformComponent = new List<(Entity entity, Vector2 startScale, Vector2 startPosition)>();
            });


            scaleTool.HorizontalDeltaStart += SetStartScale;
            scaleTool.VerticalDeltaStart += SetStartScale;

            scaleTool.HorizontalDelta += f =>
            {
                foreach (var variable in _transformComponent)
                {
                    // 1. Рассчитываем смещение относительно центра
                    float offset = variable.startPosition.x - _center.x;
                    // 2. Масштабируем смещение и прибавляем обратно к центру
                    float rawPosition = _center.x + (offset * f);

                    float newScale = gridScene.SnapToGrid(variable.startScale.x * f);

                    //Получаем матрицу трансформаций
                    PostTransformMatrix ptm = _entityManager.GetComponentData<PostTransformMatrix>(variable.entity);
                    float3 scale = GetScaleFromMatrix.Get(ptm.Value);
                    scale.x = newScale;
                    ptm.Value = float4x4.Scale(scale);
                    _entityManager.SetComponentData(variable.entity, ptm);

                    if (_coordinateSystem.IsGlobal)
                    {
                        float newPosition = gridScene.SnapToGrid(rawPosition);

                        LocalTransform localTransform =
                            _entityManager.GetComponentData<LocalTransform>(variable.entity);
                        localTransform.Position.x = newPosition;
                        
                        PositionData positionData = new PositionData();
                        positionData.Position = new float2(localTransform.Position.x, localTransform.Position.y);
                        
                        _entityManager.SetComponentData(variable.entity, positionData);
                        _entityManager.SetComponentData(variable.entity, localTransform);
                    }
                }
                
                OnValueChanged.Invoke();
            };

            scaleTool.VerticalDelta += f =>
            {
                foreach (var variable in _transformComponent)
                {
                    float offset = variable.startPosition.y - _center.y;
                    float rawPosition = _center.y + (offset * f);

                    float newScale = gridScene.SnapToGrid(variable.startScale.y * f);
                    PostTransformMatrix ptm = _entityManager.GetComponentData<PostTransformMatrix>(variable.entity);
                    float3 scale = GetScaleFromMatrix.Get(ptm.Value);
                    scale.y = newScale;
                    ptm.Value = float4x4.Scale(scale);
                    _entityManager.SetComponentData(variable.entity, ptm);

                    if (_coordinateSystem.IsGlobal)
                    {
                        float newPosition = gridScene.SnapToGrid(rawPosition);

                        LocalTransform localTransform =
                            _entityManager.GetComponentData<LocalTransform>(variable.entity);
                        localTransform.Position.y = newPosition;
                        
                        PositionData positionData = new PositionData();
                        positionData.Position = new float2(localTransform.Position.x, localTransform.Position.y);
                        _entityManager.SetComponentData(variable.entity, positionData);
                        
                        _entityManager.SetComponentData(variable.entity, localTransform);
                    }
                }
                
                OnValueChanged.Invoke();
            };

            _coordinateSystem.OnCoordinateChanged += isGlobal =>
            {
                if (isGlobal)
                    tool.position = _sceneToRawImageConverter.WorldToUIPosition(_center);
                else
                {
                    LocalTransform localTransform =
                        _entityManager.GetComponentData<LocalTransform>(_transformComponent[^1].entity);
                    tool.position =
                        _sceneToRawImageConverter.WorldToUIPosition(new Vector2(localTransform.Position.x,
                            localTransform.Position.y));
                }
            };
        }

        private void UpdatePosition()
        {
            if (_transformComponent == null || _transformComponent.Count == 0) return;

            Vector2 targetWorldPos;

            if (_coordinateSystem.IsGlobal)
            {
                // В глобальном режиме всегда в центре группы
                List<Entity> selectionOnly = _transformComponent.Select(item => item.entity).ToList();
                _center = GetCenter.GetSelectionCenter(selectionOnly);
                targetWorldPos = _center;
            }
            else
            {
                // В локальном режиме на последнем выбранном объекте
                var lastObj = _entityManager.GetComponentData<LocalTransform>(_transformComponent[^1].entity).Position;
                targetWorldPos = new Vector2(lastObj.x, lastObj.y);
            }

            tool.position = _sceneToRawImageConverter.WorldToUIPosition(targetWorldPos);

            // Вращение гизмо масштабирования
            if (!_coordinateSystem.IsGlobal && _transformComponent.Count > 0)
            {
                // Наклоняем инструмент вслед за объектом
                tool.rotation = Quaternion.Euler(0, 0,
                    GetDegree.FromQuaternion(_entityManager
                        .GetComponentData<LocalTransform>(_transformComponent[^1].entity).Rotation).z);
            }
            else
            {
                tool.rotation = Quaternion.identity;
            }
        }


        public void EnableTool()
        {
            List<Entity> selectionOnly = _transformComponent
                .Select(item => item.Item1)
                .ToList();

            _center = GetCenter.GetSelectionCenter(selectionOnly);

            if (_coordinateSystem.IsGlobal)
                tool.position = _sceneToRawImageConverter.WorldToUIPosition(_center);
            else
            {
                float3 position = _entityManager.GetComponentData<LocalTransform>(_transformComponent[^1].entity)
                    .Position;
                tool.position = _sceneToRawImageConverter.WorldToUIPosition(new Vector2(
                    position.x, position.y));
            }
        }

        void SetStartScale()
        {
            for (int i = 0; i < _transformComponent.Count; i++)
            {
                List<Entity> selectionOnly = _transformComponent
                    .Select(item => item.Item1)
                    .ToList();

                _center = GetCenter.GetSelectionCenter(selectionOnly);

                // Извлекаем компоненты для удобства расчетов
                var localTransform = _entityManager.GetComponentData<LocalTransform>(_transformComponent[i].entity) ;
                var postTransformMatrix = _entityManager.GetComponentData<PostTransformMatrix>(_transformComponent[i].entity) ;

                var scale = GetScaleFromMatrix.Get(postTransformMatrix.Value);

                float snappedX = gridScene.SnapToGrid(scale.x);
                float snappedY = gridScene.SnapToGrid(scale.y);

                float positionX = gridScene.SnapToGrid(localTransform.Position.x);
                float positionY = gridScene.SnapToGrid(localTransform.Position.y);

                // Перезаписываем кортеж целиком по индексу
                _transformComponent[i] =
                    (_transformComponent[i].entity, new Vector2(snappedX, snappedY), new Vector2(positionX, positionY));
            }
        }


        private void SetPosition(List<TrackObjectPacket> listObjects)
        {
            _transformComponent.Clear();
            foreach (var variable in listObjects)
            {
                _transformComponent.Add((variable.entity, Vector2.zero,
                    Vector2.zero));
            }

            _center = GetCenter.GetSelectionCenter(listObjects.Select(i => _entityManager.GetComponentData<LocalTransform>(i.entity)).ToList());

            tool.position = _sceneToRawImageConverter.WorldToUIPosition(_center);

            if (_transformComponent.Count <= 1)
            {
                var rotation = GetDegree.FromQuaternion(_entityManager.GetComponentData<LocalTransform>(_transformComponent[0].entity)
                    .Rotation);
                
                tool.rotation = Quaternion.Euler(tool.rotation.x, tool.rotation.y,
                    rotation.z);
            }
            else
            {
                tool.rotation = Quaternion.Euler(tool.rotation.x, tool.rotation.y, 0);
            }
        }
    }
}