using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.EditroSceneCamera;
using TimeLine.EventBus.Events.Grid;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using TimeLine.LevelEditor.ECS;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;
// ⭐ Добавлен импорт

namespace TimeLine.LevelEditor.EditorWindows.SceneView.TransformTools.Position
{
    public class PositionController : MonoBehaviour
    {
        [SerializeField] private CoordinateSystem coordinateSystem;
        [SerializeField] private PositionTool positionTool;

        [FormerlySerializedAs("camera")] [SerializeField]
        private Camera editCameraScene;

        [SerializeField] private GridScene gridScene;
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform _rectTransformPositionTool;
        [SerializeField] private Canvas toolCanvas;
        [Space] [SerializeField] private Camera mapCamera;
        [SerializeField] private RawImage rawImage;
        [SerializeField] private SceneToRawImageConverter sceneToRawImageConverter;

        private MainObjects _mainObjects;
        private GameEventBus _gameEventBus;

        private Entity _transformComponent;

        private bool _isUpdatingFromObject = false;

        private List<Entity> _otherObjects = new();
        private List<Entity> _allObjects = new();
        
        private bool _updateFromObject;

        private EntityManager _entityManager;

        public Action OnValueChanged;

        [Inject]
        private void Construct(GameEventBus eventBus, MainObjects mainObjects)
        {
            _gameEventBus = eventBus;
            _mainObjects = mainObjects;
        }


        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            _gameEventBus.SubscribeTo(((ref SelectObjectEvent data) => SelectObject(data.Tracks)));
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) => { SelectObject(data.SelectedObjects); });
            _gameEventBus.SubscribeTo((ref EditorSceneCameraUpdateViewEvent _) =>
                UpdateToolPosition());
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent _) =>
            {
                _allObjects = new List<Entity>();
                _otherObjects = new List<Entity>();
            });

            _gameEventBus.SubscribeTo(((ref GridPositionEvent data) => OnGridPositionChanged(data.StepSize)));

            coordinateSystem.OnCoordinateChanged += b =>
            {
                if (_transformComponent != null)
                {
                    _rectTransformPositionTool.rotation =
                        Quaternion.Euler(0f, 0f, b
                            ? 0f
                            : GetDegree.FromQuaternion(_entityManager
                                .GetComponentData<LocalTransform>(_transformComponent)
                                .Rotation.value).z);
                }
            };
        }

        private void UpdateToolPosition()
        {
            if (_allObjects == null || _allObjects.Count == 0) return;

            // Вычисляем текущий центр выбранных объектов в мировом пространстве
            Vector3 currentCenter =
                GetCenter.GetSelectionCenter(_allObjects.Select(i => _entityManager.GetComponentData<LocalTransform>(i))
                    .ToList());

            // Конвертируем мировые координаты сцены в координаты UI (RawImage)
            _rectTransformPositionTool.position = sceneToRawImageConverter.WorldToUIPosition(currentCenter);

            // Обновляем вращение, если мы в локальной системе координат
            if (!coordinateSystem.IsGlobal)
            {
                _rectTransformPositionTool.rotation = Quaternion.Euler(0f, 0f, GetDegree.FromQuaternion(_entityManager
                    .GetComponentData<LocalTransform>(_transformComponent)
                    .Rotation.value).z);
            }
            else
            {
                _rectTransformPositionTool.rotation = Quaternion.identity;
            }

            _updateFromObject = false;
        }


        private void OnGridPositionChanged(float newStepSize)
        {
            LocalTransform localTransform = _entityManager.GetComponentData<LocalTransform>(_transformComponent);

            // ⭐⭐⭐ ОСНОВНОЕ ИЗМЕНЕНИЕ: Получаем ТЕКУЩУЮ мировую позицию объекта
            Vector2 currentWorldPosition = new Vector2(localTransform.Position.x,
                localTransform.Position.y);

            gridScene.SetCurrentObjectPosition(currentWorldPosition);

            Quaternion currentToolGlobalRotationInverse = Quaternion.Inverse(_rectTransformPositionTool.rotation);

            Vector2 newSnappedPosition =
                gridScene.PositionFloatSnapToGrid(currentWorldPosition, currentToolGlobalRotationInverse);


            // --- Синхронизируем объект ---
            localTransform.Position.x = newSnappedPosition.x;
            localTransform.Position.y = newSnappedPosition.y;

            // --- Синхронизируем UI инструмента ---
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _mainObjects.ToolRectTransform,
                    editCameraScene.WorldToScreenPoint(newSnappedPosition),
                    editCameraScene,
                    out var snappedLocalPoint))
            {
                _rectTransformPositionTool.anchoredPosition = snappedLocalPoint;
            }
        }

        public void EnableTool()
        {
        }

        private void SelectObject(List<TrackObjectPacket> data)
        {
            //Собираем все остальные объекты
            _otherObjects = data
                .Select(i => i.entity)
                .ToList();

            _allObjects = new List<Entity>(_otherObjects);

            //Получаем последний выбранный и удаляем из списка прочих
            _transformComponent = _otherObjects[^1];
            _otherObjects.Remove(_transformComponent);

            if (_transformComponent == null)
            {
                return;
            }

            LocalTransform localTransform = _entityManager.GetComponentData<LocalTransform>(_transformComponent);

            // ⭐ Получаем ЛОКАЛЬНУЮ позицию для GridScene
            Vector2 localPositionForGrid =
                new Vector2(localTransform.Position.x, localTransform.Position.y);

            // ⭐ Получаем ГЛОБАЛЬНУЮ позицию для позиционирования UI инструмента
            Vector2 worldPositionForUI = localPositionForGrid;

            worldPositionForUI =
                GetCenter.GetSelectionCenter(_allObjects.Select(i => _entityManager.GetComponentData<LocalTransform>(i))
                    .ToList()); //todo test

            // Сообщаем GridScene текущую локальную позицию объекта (для расчета оффсета)
            gridScene.SetCurrentObjectPosition(localPositionForGrid);

            // Кэшируем последнюю снапнутую позицию (локальную, для гистерезиса)

            // --- Обновление UI инструмента с использованием ГЛОБАЛЬНОЙ позиции ---
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _mainObjects.ToolRectTransform,
                    editCameraScene.WorldToScreenPoint(worldPositionForUI), // ⭐ Используем МИРОВУЮ позицию!
                    editCameraScene,
                    out var localPoint))
            {
                return;
            }


            _rectTransformPositionTool.position =
                sceneToRawImageConverter.WorldToUIPosition(GetCenter.GetSelectionCenter(_allObjects
                    .Select(i => _entityManager.GetComponentData<LocalTransform>(i)).ToList()));


            // Устанавливаем вращение инструмента
            float targetRotation =
                coordinateSystem.IsGlobal ? 0f : GetDegree.FromQuaternion(localTransform.Rotation).z;
            _rectTransformPositionTool.rotation = Quaternion.Euler(0f, 0f, targetRotation);

            // Подписываемся на события
            positionTool.OnChangePosition -= UpdateObjectFromTool;
            positionTool.OnChangePosition += UpdateObjectFromTool;
        }

        private void UpdateObjectFromTool(RectTransform toolTransform)
        {
            if (_isUpdatingFromObject || _transformComponent == null || _updateFromObject) return;


            var smoothWorldPosition = sceneToRawImageConverter.UIToWorldPosition(toolTransform.position);
            // print(smoothWorldPosition);
            if (smoothWorldPosition == null)
            {
                return;
            }

            var smoothWorldPosition3D = (Vector3)smoothWorldPosition;


            // 3. ПРИМЕНЯЕМ ВРАЩЕНИЕ (как в твоем исходном коде)
            float rotationAnglee = -toolTransform.localEulerAngles.z;
            Quaternion inverseRotation = Quaternion.Euler(0, 0, rotationAnglee);

            // 4. СНАПИНГ И ОБНОВЛЕНИЕ ОБЪЕКТОВ
            // Сохраняем старый центр группы для вычисления дельты
            var oldPosition =
                GetCenter.GetSelectionCenter(_allObjects.Select(i => _entityManager.GetComponentData<LocalTransform>(i))
                    .ToList());

            // Получаем новую позицию со снапом к сетке
            Vector2 candidateSnappedPosition =
                gridScene.PositionFloatSnapToGrid(smoothWorldPosition3D, inverseRotation);

            // Вычисляем разницу (смещение)
            Vector2 differentPosition = candidateSnappedPosition - (Vector2)oldPosition;

            foreach (var otherObject in _allObjects)
            {
                // 1. Проверяем, существует ли сущность (безопасность)
                if (!_entityManager.Exists(otherObject)) continue;

                // 2. Получаем КОПИЮ данных
                LocalTransform localTransform = _entityManager.GetComponentData<LocalTransform>(otherObject);

                // 3. Изменяем локальную копию
                localTransform.Position.x += differentPosition.x;
                localTransform.Position.y += differentPosition.y;

                // 4. ЗАПИСЫВАЕМ измененную структуру обратно в ECS
                _entityManager.SetComponentData(otherObject, localTransform);
                PositionData positionData = new PositionData();
                positionData.Position = new float2(localTransform.Position.x, localTransform.Position.y);
                _entityManager.SetComponentData(otherObject, positionData);
            }

            _rectTransformPositionTool.position =
                sceneToRawImageConverter.WorldToUIPosition(GetCenter.GetSelectionCenter(_allObjects
                    .Select(i => _entityManager.GetComponentData<LocalTransform>(i)).ToList()));
            
            OnValueChanged?.Invoke();
        }
    }
}