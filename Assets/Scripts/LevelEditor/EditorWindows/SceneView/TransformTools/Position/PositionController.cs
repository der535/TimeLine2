using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.EditroSceneCamera;
using TimeLine.EventBus.Events.Grid; // ⭐ Добавлен импорт
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class PositionController : MonoBehaviour
    {
        [SerializeField] private CoordinateSystem coordinateSystem;
        [SerializeField] private PositionTool positionTool;
        [FormerlySerializedAs("camera")] [SerializeField] private Camera editCameraScene;
        [SerializeField] private GridScene gridScene;
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform _rectTransformPositionTool;
        [SerializeField] private Canvas toolCanvas;
        [Space] [SerializeField] private Camera mapCamera;
        [SerializeField] private RawImage rawImage;
        [SerializeField] private SceneToRawImageConverter sceneToRawImageConverter;
        
        private MainObjects _mainObjects;
        private GameEventBus _gameEventBus;

        private TransformComponent _transformComponent;

        private bool _isUpdatingFromObject = false;
        
        private List<TransformComponent> _otherObjects = new();
        private List<TransformComponent> _allObjects = new();
        
        private Action _toolFollowingObject;
        private bool _updateFromObject;

        [Inject]
        private void Construct(GameEventBus eventBus, MainObjects mainObjects)
        {
            _gameEventBus = eventBus;
            _mainObjects = mainObjects;
        }


        private void Start()
        {
            _toolFollowingObject += () =>
            {
                _updateFromObject = true;
                UpdateToolPosition();
            };
            _gameEventBus.SubscribeTo(((ref SelectObjectEvent data) => SelectObject(data.Tracks)));
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) =>
            {
                SelectObject(data.SelectedObjects);
            });
            _gameEventBus.SubscribeTo((ref EditorSceneCameraUpdateViewEvent sceneCameraUpdateViewEvent) =>  UpdateToolPosition());
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) =>
            {
                _allObjects = new List<TransformComponent>();
                _otherObjects = new List<TransformComponent>();
            });

            _gameEventBus.SubscribeTo(((ref GridPositionEvent data) => OnGridPositionChanged(data.StepSize)));

            coordinateSystem.OnCoordinateChanged += b =>
            {
                _rectTransformPositionTool.rotation =
                    Quaternion.Euler(0f, 0f, b ? 0f : _transformComponent?.ZRotation.Value ?? 0f);
            };
        }

        private void UpdateToolPosition()
        {
            if (_allObjects == null || _allObjects.Count == 0) return;

            // Вычисляем текущий центр выбранных объектов в мировом пространстве
            Vector3 currentCenter = GetCenter.GetSelectionCenter(_allObjects.Select(i => i.transform).ToList());

            // Конвертируем мировые координаты сцены в координаты UI (RawImage)
            _rectTransformPositionTool.position = sceneToRawImageConverter.WorldToUIPosition(currentCenter);

            // Обновляем вращение, если мы в локальной системе координат
            if (!coordinateSystem.IsGlobal && _transformComponent != null)
            {
                _rectTransformPositionTool.rotation = Quaternion.Euler(0f, 0f, _transformComponent.transform.rotation.eulerAngles.z);
            }
            else
            {
                _rectTransformPositionTool.rotation = Quaternion.identity;
            }

            _updateFromObject = false;
        }
        

        private void OnGridPositionChanged(float newStepSize)
        {
            if (_transformComponent == null)
            {
                return;
            }

            // ⭐⭐⭐ ОСНОВНОЕ ИЗМЕНЕНИЕ: Получаем ТЕКУЩУЮ мировую позицию объекта
            Vector2 currentWorldPosition = new Vector2(_transformComponent.transform.position.x,
                _transformComponent.transform.position.y);

            gridScene.SetCurrentObjectPosition(currentWorldPosition);

            Quaternion currentToolGlobalRotationInverse = Quaternion.Inverse(_rectTransformPositionTool.rotation);

            Vector2 newSnappedPosition =
                gridScene.PositionFloatSnapToGrid(currentWorldPosition, currentToolGlobalRotationInverse);


            // --- Синхронизируем объект ---
            _transformComponent.XPosition.Value = newSnappedPosition.x;
            _transformComponent.YPosition.Value = newSnappedPosition.y;

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
            foreach (var VARIABLE in _allObjects)
            {
                if(VARIABLE == null || VARIABLE.XPosition == null) return;
                VARIABLE.XPosition.OnValueChanged -= _toolFollowingObject;
                VARIABLE.YPosition.OnValueChanged -= _toolFollowingObject;
            }

            
            //Собираем все остальные объекты
            _otherObjects = data
                .Select(i => i.sceneObject.GetComponent<TransformComponent>())
                .Where(comp => comp != null)
                .ToList();

            _allObjects = new List<TransformComponent>(_otherObjects);

            //Получаем последний выбранный и удаляем из списка прочих
            _transformComponent = _otherObjects[^1];
            _otherObjects.Remove(_transformComponent);

            if (_transformComponent == null)
            {
                return;
            }

            foreach (var VARIABLE in _allObjects)
            {
                VARIABLE.XPosition.OnValueChanged += _toolFollowingObject;
                VARIABLE.YPosition.OnValueChanged += _toolFollowingObject;
            }

            // ⭐ Получаем ЛОКАЛЬНУЮ позицию для GridScene
            Vector2 localPositionForGrid =
                new Vector2(_transformComponent.XPosition.Value, _transformComponent.YPosition.Value);

            // ⭐ Получаем ГЛОБАЛЬНУЮ позицию для позиционирования UI инструмента
            Vector2 worldPositionForUI = new Vector2(_transformComponent.transform.position.x,
                _transformComponent.transform.position.y);

            worldPositionForUI =
                GetCenter.GetSelectionCenter(_allObjects.Select(i => i.transform).ToList()); //todo test

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
                sceneToRawImageConverter.WorldToUIPosition( GetCenter.GetSelectionCenter(_allObjects.Select(i => i.transform).ToList()));


            // Устанавливаем вращение инструмента
            float targetRotation =
                coordinateSystem.IsGlobal ? 0f : _transformComponent.transform.rotation.eulerAngles.z;
            _rectTransformPositionTool.rotation = Quaternion.Euler(0f, 0f, targetRotation);

            // Подписываемся на события
            positionTool.OnChangePosition -= UpdateObjectFromTool;
            positionTool.OnChangePosition += UpdateObjectFromTool;
        }

        private void UpdateObjectFromTool(RectTransform toolTransform)
        {
            if (_isUpdatingFromObject || _transformComponent == null || _updateFromObject ) return;
            

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
            var oldPosition = GetCenter.GetSelectionCenter(_allObjects.Select(i => i.transform).ToList());

            // Получаем новую позицию со снапом к сетке
            Vector2 candidateSnappedPosition =
                gridScene.PositionFloatSnapToGrid(smoothWorldPosition3D, inverseRotation);

            // Вычисляем разницу (смещение)
            Vector2 differentPosition = candidateSnappedPosition - (Vector2)oldPosition;


            // Применяем смещение ко всей группе объектов
            foreach (var otherObject in _allObjects)
            {
                otherObject.XPosition.Value += differentPosition.x;
                otherObject.YPosition.Value += differentPosition.y;
            }

            _rectTransformPositionTool.position =
                sceneToRawImageConverter.WorldToUIPosition( GetCenter.GetSelectionCenter(_allObjects.Select(i => i.transform).ToList()));
        }
    }
}