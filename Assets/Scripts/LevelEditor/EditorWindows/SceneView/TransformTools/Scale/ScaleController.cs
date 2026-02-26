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

namespace TimeLine
{
    public class ScaleController : MonoBehaviour
    {
        [FormerlySerializedAs("_gridScene")] [SerializeField]
        private GridScene gridScene;

        [FormerlySerializedAs("edit_camera_UI")] [FormerlySerializedAs("camera")] [SerializeField] private Camera editCameraUI;
        [SerializeField] private RectTransform tool;
        [FormerlySerializedAs("_scaleTool")] [SerializeField] private ScaleTool scaleTool;

        private GameEventBus _eventBus;
        private MainObjects _mainObjects;
        private SceneToRawImageConverter _sceneToRawImageConverter;
        private CoordinateSystem _coordinateSystem;

        private List<(TransformComponent transformComponent, Vector2 startScale, Vector2 startPosition)> _transformComponent = new();

        private Action _objectFollowingTool;
        private Action _toolFollowingObject;

        private Action<float> _horizontalScale;
        private Action<float> _verticalScale;

        private Vector2 center;

        [Inject]
        private void Construct(GameEventBus gameEventBus, MainObjects mainObjects,
            SceneToRawImageConverter sceneToRawImageConverter, CoordinateSystem coordinateSystem)
        {
            _eventBus = gameEventBus;
            _mainObjects = mainObjects;
            _sceneToRawImageConverter = sceneToRawImageConverter;
            _coordinateSystem = coordinateSystem;
        }

        private void Awake()
        {
            _eventBus.SubscribeTo(((ref SelectObjectEvent data) =>
            {
                SetPosition(data.Tracks);
            }));
            _eventBus.SubscribeTo((ref DeselectObjectEvent data) =>
            {
                SetPosition(data.SelectedObjects);
            });
            _eventBus.SubscribeTo((ref EditorSceneCameraUpdateViewEvent data) =>
            {
                UpdatePosition();
            });
            _eventBus.SubscribeTo((ref DeselectAllObjectEvent data) =>
            {
                _transformComponent = new List<(TransformComponent transformComponent, Vector2 startScale, Vector2 startPosition)>();
            });

            _toolFollowingObject += () => UpdatePosition();
            
            scaleTool.HorizontalDeltaStart += SetStartScale;
            scaleTool.VerticalDeltaStart += SetStartScale;

            scaleTool.HorizontalDelta += f =>
            {
                foreach (var variable in _transformComponent)
                {
                    // 1. Рассчитываем смещение относительно центра
                    float offset = variable.startPosition.x - center.x;
                    // 2. Масштабируем смещение и прибавляем обратно к центру
                    float rawPosition = center.x + (offset * f);
        
                    float newScale = gridScene.SnapToGrid(variable.startScale.x * f);
                    variable.transformComponent.XScale.Value = newScale;

                    if (_coordinateSystem.IsGlobal)
                    {
                        float newPosition = gridScene.SnapToGrid(rawPosition);
                        variable.transformComponent.XPosition.Value = newPosition; 
                    }

                }
            };

            scaleTool.VerticalDelta += f =>
            {
                foreach (var variable in _transformComponent)
                {
                    float offset = variable.startPosition.y - center.y;
                    float rawPosition = center.y + (offset * f);
        
                    float newScale = gridScene.SnapToGrid(variable.startScale.y * f);
                    variable.transformComponent.YScale.Value = newScale;

                    if (_coordinateSystem.IsGlobal)
                    {
                        float newPosition = gridScene.SnapToGrid(rawPosition);
                        variable.transformComponent.YPosition.Value = newPosition;
                    }
                }
            };

            _coordinateSystem.OnCoordinateChanged += isGlobal =>
            {
                if(isGlobal)
                    tool.position = _sceneToRawImageConverter.WorldToUIPosition(center);
                else
                {
                    tool.position = _sceneToRawImageConverter.WorldToUIPosition(new Vector2(_transformComponent[^1].transformComponent.XPosition.Value, _transformComponent[^1].transformComponent.YPosition.Value));
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
                List<TransformComponent> selectionOnly = _transformComponent.Select(item => item.transformComponent).ToList();
                center = GetCenter.GetSelectionCenter(selectionOnly);
                targetWorldPos = center;
            }
            else
            {
                // В локальном режиме на последнем выбранном объекте
                var lastObj = _transformComponent[^1].transformComponent;
                targetWorldPos = new Vector2(lastObj.XPosition.Value, lastObj.YPosition.Value);
            }

            tool.position = _sceneToRawImageConverter.WorldToUIPosition(targetWorldPos);

            // Вращение гизмо масштабирования
            if (!_coordinateSystem.IsGlobal && _transformComponent.Count > 0)
            {
                // Наклоняем инструмент вслед за объектом
                tool.rotation = Quaternion.Euler(0, 0, _transformComponent[^1].transformComponent.ZRotation.Value);
            }
            else
            {
                tool.rotation = Quaternion.identity;
            }
        }
        
        
        public void EnableTool()
        {
            List<TransformComponent> selectionOnly = _transformComponent
                .Select(item => item.Item1)
                .ToList();

            center = GetCenter.GetSelectionCenter(selectionOnly);
            
            if(_coordinateSystem.IsGlobal)
                tool.position = _sceneToRawImageConverter.WorldToUIPosition(center);
            else
            {
                tool.position = _sceneToRawImageConverter.WorldToUIPosition(new Vector2(_transformComponent[^1].transformComponent.XPosition.Value, _transformComponent[^1].transformComponent.YPosition.Value));
            }
        }

        void SetStartScale()
        {
            for (int i = 0; i < _transformComponent.Count; i++)
            {
                List<TransformComponent> selectionOnly = _transformComponent
                    .Select(item => item.Item1)
                    .ToList();

                center = GetCenter.GetSelectionCenter(selectionOnly);
                
                // Извлекаем компоненты для удобства расчетов
                var component = _transformComponent[i].transformComponent;

                float snappedX = gridScene.SnapToGrid(component.XScale.Value);
                float snappedY = gridScene.SnapToGrid(component.YScale.Value);
                
                float positionX = gridScene.SnapToGrid(component.XPosition.Value);
                float positionY = gridScene.SnapToGrid(component.YPosition.Value);

                // Перезаписываем кортеж целиком по индексу
                _transformComponent[i] = (component, new Vector2(snappedX, snappedY), new Vector2(positionX, positionY));
            }
        }
        

        private void SetPosition(List<TrackObjectPacket> listObjects)
        {
            foreach (var VARIABLE in _transformComponent)
            {
                if(VARIABLE.Item1.XPosition == null || VARIABLE.Item1.YPosition == null) continue;

                VARIABLE.Item1.XPosition.OnValueChanged -= _toolFollowingObject;
                VARIABLE.Item1.YPosition.OnValueChanged -= _toolFollowingObject;
            }

            _transformComponent.Clear();
            foreach (var variable in listObjects)
            {
                _transformComponent.Add((variable.sceneObject.GetComponent<TransformComponent>(), Vector2.zero, Vector2.zero));
            }

            center = GetCenter.GetSelectionCenter(listObjects.Select(i => i.sceneObject.transform).ToList());
            
            tool.position = _sceneToRawImageConverter.WorldToUIPosition(center);
            
            if (_transformComponent.Count <= 1)
            {
                tool.rotation = Quaternion.Euler(tool.rotation.x, tool.rotation.y,
                    _transformComponent[0].Item1.ZRotation.Value);
            }
            else
            {
                tool.rotation = Quaternion.Euler(tool.rotation.x, tool.rotation.y, 0);
            }

            foreach (var VARIABLE in _transformComponent)
            {
                VARIABLE.Item1.XPosition.OnValueChanged += _toolFollowingObject;
                VARIABLE.Item1.YPosition.OnValueChanged += _toolFollowingObject;
            }
        }
    }
}