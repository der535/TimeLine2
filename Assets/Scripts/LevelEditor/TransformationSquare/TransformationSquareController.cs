using System;
using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.Cursor;
using TimeLine.EventBus.Events.EditroSceneCamera;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.ECS;
using TimeLine.LevelEditor.ECS.Services;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using TimeLine.LevelEditor.TransformationSquare.Service;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.TransformationSquare
{
    public class TransformationSquareController : MonoBehaviour
    {
        [SerializeField] private TransformationSquareView view;
        private TransformationSquareData _data = new();

        private TransformationSquareMouseDistanceCheck _mouseDistanceCheck;
        private TransformationSquareMousePosition _mousePosition;
        private TransformationSquareUpdateSquare _updateSquare;
        private TransformationSquareMouseClick _mouseClick;

        private SceneToRawImageConverter _sceneToRawImageConverter;
        private GameEventBus _gameEventBus;
        private CameraReferences _cameraReferences;
        private CursorController _cursorController;
        private ActionMap _actionMap;
        private List<Entity> _selectedEntits = new List<Entity>();

        public Action OnStopPositionY;
        public Action OnStopPositionX;
        public Action OnStopScaleX;
        public Action OnStopScaleY;
        public Action OnStopRotation;

        [Inject]
        private void Construct(SceneToRawImageConverter sceneToRawImageConverter, GameEventBus eventBus,
            CameraReferences references, CursorController cursorController, ActionMap actionMap)
        {
            _sceneToRawImageConverter = sceneToRawImageConverter;
            _gameEventBus = eventBus;
            _cameraReferences = references;
            _cursorController = cursorController;
            _actionMap = actionMap;
        }

        private void Awake()
        {
            _mousePosition = new TransformationSquareMousePosition(view, _cameraReferences);
            _mouseDistanceCheck =
                new TransformationSquareMouseDistanceCheck(_mousePosition, _data, _sceneToRawImageConverter);
            _updateSquare = new TransformationSquareUpdateSquare(_sceneToRawImageConverter, view, _data);
            _mouseClick = new TransformationSquareMouseClick(_data, _sceneToRawImageConverter, _mouseDistanceCheck);
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                _selectedEntits = data.Tracks.Select(x => x.entity).ToList();
                _updateSquare.UpdateGroupOBB(data.Tracks.Select(x => x.entity).ToList());
            });
            
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) =>
            {
                _selectedEntits = data.SelectedObjects.Select(x => x.entity).ToList();
                _updateSquare.UpdateGroupOBB(data.SelectedObjects.Select(x => x.entity).ToList());
            });

            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) =>
            {
                _selectedEntits.Clear();
            });
            
            _gameEventBus.SubscribeTo((ref EditorSceneCameraUpdateViewEvent data) =>
            {
                _updateSquare.UpdateGroupOBB(_data._selectedEntities.Select(x => x.Entity).ToList(), true);
            });

            _actionMap.Editor.MouseLeft.started += context =>
            {
                _mouseClick.Click(_selectedEntits);
            };

            _actionMap.Editor.MouseLeft.canceled += _ =>
            {
                foreach (var entity in _data._selectedEntities)
                {
                    EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                    LocalTransform localTransform = manager.GetComponentData<LocalTransform>(entity.Entity);
                    PostTransformMatrix postTransform = manager.GetComponentData<PostTransformMatrix>(entity.Entity);
                    RotationData rotationData = manager.GetComponentData<RotationData>(entity.Entity);
                    var scale = GetScaleFromMatrix.Get(postTransform.Value);

                    if (!Mathf.Approximately(entity.InitialWorldPos.x, localTransform.Position.x))
                    {
                        OnStopPositionX?.Invoke();
                    }

                    if (!Mathf.Approximately(entity.InitialWorldPos.y, localTransform.Position.y))
                    {
                        OnStopPositionY?.Invoke();
                    }

                    if (!Mathf.Approximately(scale.x, entity.InitialScale.x))
                    {
                        OnStopScaleX?.Invoke();
                    }

                    if (!Mathf.Approximately(scale.y, entity.InitialScale.y))
                    {
                        OnStopScaleY?.Invoke();
                    }

                    if (!Mathf.Approximately(GetDegree.FromQuaternion(localTransform.Rotation).z,
                            GetDegree.FromQuaternion(entity.InitialRotation).z))
                    {
                        rotationData.RotateZ = GetDegree.FromQuaternion(localTransform.Rotation).z;
                        manager.SetComponentData(entity.Entity, rotationData);
                        OnStopRotation?.Invoke();
                    }
                }
                
                _data.IsResizingRight = false;
                _data.IsResizingLeft = false;
                _data.IsResizingUp = false;
                _data.IsResizingDown = false;
                _data.IsRotating = false;
                _data.IsDragging = false;
                
                _updateSquare.UpdateGroupOBB(_selectedEntits);
            };
        }


        private void Update()
        {
            if (_mouseDistanceCheck.RightLine() || _mouseDistanceCheck.LeftLine())
                _cursorController.SetResizeHorizontal();
            else if (_mouseDistanceCheck.UpBorder() || _mouseDistanceCheck.MouseInResizeAreaDown())
                _cursorController.SetResizeVertical();
            else _cursorController.SetIdel();

            if (_mouseDistanceCheck.TopLeftCorner()) _cursorController.SetResizeDiagonallyLeft();
            if (_mouseDistanceCheck.TopRightCorner()) _cursorController.SetResizeDiagonallyRight();
            if (_mouseDistanceCheck.BottomRightCorner()) _cursorController.SetResizeDiagonallyLeft();
            if (_mouseDistanceCheck.BottomLeftCorner()) _cursorController.SetResizeDiagonallyRight();
            if (_mouseDistanceCheck.CheckMouseAllPointsDistanceToRotate()) _cursorController.SetHover();

            if (_data.IsResizingLeft && _data.IsResizingUp)
            {
                ReziseObject(_data.InitialBoxLocalMax.x, _data.InitialBoxLocalMin.y, false, true);
            }

            if (_data.IsResizingRight && _data.IsResizingUp)
            {
                ReziseObject(_data.InitialBoxLocalMin.x, _data.InitialBoxLocalMin.y, true, true);
            }

            if (_data.IsResizingRight && _data.IsResizingDown)
            {
                ReziseObject(_data.InitialBoxLocalMin.x, _data.InitialBoxLocalMax.y, true, false);
            }

            if (_data.IsResizingLeft && _data.IsResizingDown)
            {
                ReziseObject(_data.InitialBoxLocalMax.x, _data.InitialBoxLocalMax.y, false, false);
            }

            // В методе Update, когда считаете масштаб для правой стороны:
            if (_data.IsResizingRight && !_data.IsResizingDown && !_data.IsResizingUp)
            {
                ReziseObject(_data.InitialBoxLocalMin.x, 0, true, true, applyScaleY: false);
            }

            if (_data.IsResizingLeft && !_data.IsResizingDown && !_data.IsResizingUp)
            {
                ReziseObject(_data.InitialBoxLocalMax.x, 0, false, true, applyScaleY: false);
            }

            if (_data.IsResizingUp && !_data.IsResizingLeft && !_data.IsResizingRight)
            {
                ReziseObject(0, _data.InitialBoxLocalMin.y, true, true, applyScaleX: false);
            }

            if (_data.IsResizingDown && !_data.IsResizingLeft && !_data.IsResizingRight)
            {
                ReziseObject(0, _data.InitialBoxLocalMax.y, true, false, applyScaleX: false);
            }

            // В конце метода Click после всех проверок на Resize и Rotate:
            if (_data.IsDragging)
            {
                EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

                float3 currentMouseWorld = _sceneToRawImageConverter.GetWorldPositionFromMouseOnRawImage();
                float3 startMouseWorld = new float3(_data.LastMousePosition.x, _data.LastMousePosition.y, 0);

                // 1. Считаем чистую мировую дельту
                float3 moveDelta = currentMouseWorld - startMouseWorld;

                if (_actionMap.Editor.LeftShift.IsPressed())
                {
                    // 2. Переводим мировую дельту в локальное направление рамки
                    // Используем только вращение (rotate), так как дельта — это вектор, а не точка
                    if (_actionMap.Editor.LeftAlt.IsPressed())
                    {
                        float3 localDelta = math.rotate(math.inverse(_data.PivotToWorldMatrix.Rotation()), moveDelta);


                        // 3. Выбираем доминирующую локальную ось
                        if (math.abs(localDelta.x) > math.abs(localDelta.y))
                        {
                            localDelta.y = 0;
                        }
                        else
                        {
                            localDelta.x = 0;
                        }

                        // 4. Переводим локальную дельту обратно в мировую
                        moveDelta = math.rotate(_data.PivotToWorldMatrix.Rotation(), localDelta);
                    }
                    else
                    {
                        // 3. Выбираем доминирующую локальную ось
                        if (math.abs(moveDelta.x) > math.abs(moveDelta.y))
                        {
                            moveDelta.y = 0;
                        }
                        else
                        {
                            moveDelta.x = 0;
                        }
                    }
                }

                foreach (var snap in _data._selectedEntities)
                {
                    // 5. Применяем уже скорректированную дельту
                    float3 newWorldPos = snap.InitialWorldPos + moveDelta;

                    var lt = em.GetComponentData<LocalTransform>(snap.Entity);
                    lt.Position = newWorldPos;
                    em.SetComponentData(snap.Entity, lt);
                }

                // ВАЖНО: Прибавление moveDelta к центру должно быть после коррекции
                // Но так как мы используем InitialWorldPos, центр тоже лучше считать от стартового
                _updateSquare.UpdateGroupOBB(_selectedEntits, false);
            }

            if (_data.IsRotating)
            {
                EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
                float3 mouseWorld = _sceneToRawImageConverter.GetWorldPositionFromMouseOnRawImage();

                // 1. Считаем угол относительно ТОГО ЖЕ центра, что и при клике
                float currentMouseAngle =
                    math.atan2(mouseWorld.y - _data.GroupCenter.y, mouseWorld.x - _data.GroupCenter.x);
                float angleDelta = currentMouseAngle - _data.InitialMouseAngle;
                quaternion rotationOffset = quaternion.AxisAngle(new float3(0, 0, 1), angleDelta);

                foreach (var snap in _data._selectedEntities)
                {
                    // 2. Вращаем сохраненный "рычаг" (офсет)
                    float3 rotatedOffset = math.rotate(rotationOffset, snap.RotationOffsetWorld);

                    // 3. Новая позиция = Центр + повернутый рычаг
                    float3 newWorldPos = _data.GroupCenter + rotatedOffset;

                    // 4. Новый поворот самого объекта
                    quaternion newWorldRot = math.mul(rotationOffset, snap.InitialRotation);

                    // 5. Запись в ECS
                    var lt = em.GetComponentData<LocalTransform>(snap.Entity);
                    lt.Position = newWorldPos;
                    lt.Rotation = newWorldRot;

                    em.SetComponentData(snap.Entity, lt);
                }

                // После вращения всех объектов обновляем рамку OBB
                _updateSquare.UpdateGroupOBB(_selectedEntits, false);
            }
        }


        void ReziseObject(float pivotX, float pivotY, bool scaleXPlusDelta, bool scaleYPlusDelta,
            bool applyScaleX = true,
            bool applyScaleY = true)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            float3 mouseWorld = _sceneToRawImageConverter.GetWorldPositionFromMouseOnRawImage();
            float3 mouseLocal = math.transform(_data.WorldToPivotMatrix, mouseWorld);
            float3 startMouseLocal = math.transform(_data.WorldToPivotMatrix,
                new float3(_data.LastMousePosition.x, _data.LastMousePosition.y, 0));

            float deltaX = mouseLocal.x - startMouseLocal.x;
            float deltaY = mouseLocal.y - startMouseLocal.y;

            // Инициализируем множители единицей (без изменений)
            float scaleX = 1.0f;
            float scaleY = 1.0f;

            // Считаем масштаб только если ось активна
            if (applyScaleX)
            {
                if (scaleXPlusDelta)
                    scaleX = (_data.InitialBoxSize.x + deltaX) / _data.InitialBoxSize.x;
                else
                    scaleX = (_data.InitialBoxSize.x - deltaX) / _data.InitialBoxSize.x;

                scaleX = math.max(scaleX, 0.01f);
            }

            if (applyScaleY)
            {
                if (scaleYPlusDelta)
                    scaleY = (_data.InitialBoxSize.y + deltaY) / _data.InitialBoxSize.y;
                else
                    scaleY = (_data.InitialBoxSize.y - deltaY) / _data.InitialBoxSize.y;

                scaleY = math.max(scaleY, 0.01f);
            }

            foreach (var snap in _data._selectedEntities)
            {
                float3 newScale = snap.InitialScale;
                // Скейлим только нужные оси в самом компоненте
                if (applyScaleX) newScale.x *= scaleX;
                if (applyScaleY) newScale.y *= scaleY;

                float3 newLocalPos = snap.LocalPosInBox;

                // ВАЖНО: Если scaleX или scaleY равны 1.0f, 
                // то новые координаты будут равны старым (pivot + offset)
                float offsetX = snap.LocalPosInBox.x - pivotX;
                newLocalPos.x = pivotX + (offsetX * scaleX);

                float offsetY = snap.LocalPosInBox.y - pivotY;
                newLocalPos.y = pivotY + (offsetY * scaleY);

                float3 newWorldPos = math.transform(_data.PivotToWorldMatrix, newLocalPos);

                var lt = entityManager.GetComponentData<LocalTransform>(snap.Entity);
                lt.Position = newWorldPos;

                var ptm = entityManager.GetComponentData<PostTransformMatrix>(snap.Entity);
                ptm.Value = float4x4.Scale(newScale);

                entityManager.SetComponentData(snap.Entity, lt);
                entityManager.SetComponentData(snap.Entity, ptm);
            }

            _updateSquare.UpdateGroupOBB(_selectedEntits, false);
        }
    }
}