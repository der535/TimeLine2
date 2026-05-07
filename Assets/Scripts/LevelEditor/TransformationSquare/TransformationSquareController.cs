using System;
using System.Collections;
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
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.LevelEditor.TransformationSquare
{
    public class TransformationSquareController : MonoBehaviour
    {
        [SerializeField] private TransformationSquareView view;
        private TransformationSquareData _data = new();
        public RawImage mapImage;

        private TransformationSquareMouseDistanceCheck _mouseDistanceCheck;
        private TransformationSquareMousePosition _mousePosition;
        private TransformationSquareUpdateSquare _updateSquare;
        private TransformationSquareMouseClick _mouseClick;

        private SceneToRawImageConverter _sceneToRawImageConverter;
        private GameEventBus _gameEventBus;
        private CameraReferences _cameraReferences;
        private CursorController _cursorController;
        private ActionMap _actionMap;
        private List<Entity> _selectedEntits = new();

        public bool isEditing;

        public Action OnValueChange;

        public Action OnStopPositionY;
        public Action OnStopPositionX;
        public Action OnStopScaleX;
        public Action OnStopScaleY;
        public Action OnStopRotation;

        [FormerlySerializedAs("_activeToll")]
        public bool activeToll;


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
            view.SetActive(false);
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
                if (data.UpdateVisual)
                {
                    if (activeToll) view.SetActive(true);
                    _selectedEntits = data.Tracks.Select(x => x.entity).ToList();
                    _updateSquare.UpdateGroupOBB(data.Tracks.Select(x => x.entity).ToList());
                    _mouseClick.UpdateSelectedEntities(_selectedEntits);
                }
            });

            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) =>
            {
                _selectedEntits = data.SelectedObjects.Select(x => x.entity).ToList();
                _updateSquare.UpdateGroupOBB(data.SelectedObjects.Select(x => x.entity).ToList());
                _mouseClick.UpdateSelectedEntities(_selectedEntits);
            });

            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) =>
            {
                view.SetActive(false);
                _selectedEntits.Clear();
                _mouseClick.UpdateSelectedEntities(_selectedEntits);
            });

            _gameEventBus.SubscribeTo((ref EditorSceneCameraUpdateViewEvent _) => { _updateSquare.UpdateGroupOBB(_data._selectedEntities.Select(x => x.Entity).ToList(), true); });

            _actionMap.Editor.MouseLeft.started += _ =>
            {
                _mouseClick.UpdateSelectedEntities(_selectedEntits);

                _mouseClick.Click(_selectedEntits);

                if (_data.GetIsEditingObject() && activeToll) isEditing = true;
            };

            _actionMap.Editor.MouseLeft.canceled += _ =>
            {
                if (!_data.GetIsEditingObject()) return;

                StartCoroutine(SetEditingStateFalse());

                foreach (var entity in _data._selectedEntities)
                {
                    EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                    LocalTransform localTransform = manager.GetComponentData<LocalTransform>(entity.Entity);
                    PostTransformMatrix postTransform = manager.GetComponentData<PostTransformMatrix>(entity.Entity);
                    RotationData rotationData = manager.GetComponentData<RotationData>(entity.Entity);
                    var scale = GetScaleFromMatrix.Get(postTransform.Value);

                    if (!Mathf.Approximately(entity.InitialWorldPos.x, localTransform.Position.x))
                        OnStopPositionX?.Invoke();

                    if (!Mathf.Approximately(entity.InitialWorldPos.y, localTransform.Position.y))
                        OnStopPositionY?.Invoke();

                    if (!Mathf.Approximately(scale.x, entity.InitialScale.x))
                        OnStopScaleX?.Invoke();

                    if (!Mathf.Approximately(scale.y, entity.InitialScale.y))
                        OnStopScaleY?.Invoke();

                    if (!Mathf.Approximately(rotationData.RotateZ, entity.InitialRotation)) // Предполагая, что вы сохранили float в InitialRotationValue
                    {
                        // Мы не берем угол из трансформа! Он уже должен быть правильно записан в RotationData во время Update
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

            _mouseClick.UpdateSelectedEntities(_selectedEntits);
        }


        private void Update()
        {
            if (view.GetActive() == false) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    mapImage.rectTransform, UnityEngine.Input.mousePosition, _cameraReferences.editUICamera,
                    out Vector2 localPoint))
                return;

            if (!mapImage.rectTransform.rect.Contains(localPoint) && !_data.GetIsEditingObject())
            {
                // Мышь за пределами границ картинки
                return;
            }


            // В начале блока проверки курсора
            float currentRotation = GetDegree.FromQuaternion(_data.PivotToWorldMatrix.Rotation()).z;

            WindowsCursorID nextState = WindowsCursorID.Arrow;

// Проверка линий (Стороны)
            if (_mouseDistanceCheck.RightLine() || _mouseDistanceCheck.LeftLine())
                nextState = GetRotatedSizeCursor(WindowsCursorID.SizeWE, currentRotation);
            else if (_mouseDistanceCheck.UpBorder() || _mouseDistanceCheck.MouseInResizeAreaDown())
                nextState = GetRotatedSizeCursor(WindowsCursorID.SizeNS, currentRotation);

// Проверка углов
            if (_mouseDistanceCheck.TopLeftCorner())
                nextState = GetRotatedSizeCursor(WindowsCursorID.SizeNWSE, currentRotation);
            else if (_mouseDistanceCheck.TopRightCorner())
                nextState = GetRotatedSizeCursor(WindowsCursorID.SizeNESW, currentRotation);
            else if (_mouseDistanceCheck.BottomRightCorner())
                nextState = GetRotatedSizeCursor(WindowsCursorID.SizeNWSE, currentRotation);
            else if (_mouseDistanceCheck.BottomLeftCorner())
                nextState = GetRotatedSizeCursor(WindowsCursorID.SizeNESW, currentRotation);

// Вращение и перемещение
            if (_mouseDistanceCheck.CheckMouseAllPointsDistanceToRotate())
                nextState = WindowsCursorID.Hand;

            _cursorController.SetState(nextState);

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
                    var pd = em.GetComponentData<PositionData>(snap.Entity);
                    lt.Position = new float3(newWorldPos.x, newWorldPos.y, lt.Position.z);
                    pd.Position = new float2(newWorldPos.x, newWorldPos.y);
                    em.SetComponentData(snap.Entity, lt);
                    em.SetComponentData(snap.Entity, pd);
                }

                // ВАЖНО: Прибавление moveDelta к центру должно быть после коррекции
                // Но так как мы используем InitialWorldPos, центр тоже лучше считать от стартового
                _updateSquare.UpdateGroupOBB(_selectedEntits, false);
            }

            if (_data.IsRotating)
            {
                EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
                float3 mouseWorld = _sceneToRawImageConverter.GetWorldPositionFromMouseOnRawImage();

                // 1. Используем стабильный центр, который был зафиксирован ПРИ КЛИКЕ
                // (Убедитесь, что вы сохранили его в _data.InitialGroupCenter при старте вращения)
                float3 pivot = _data.GroupCenter;

                // 2. Считаем текущий угол
                float currentMouseAngle = math.atan2(mouseWorld.y - pivot.y, mouseWorld.x - pivot.x);

                // 3. Считаем разницу относительно ПРЕДЫДУЩЕГО кадра
                if (!_data.WasRotatingLastFrame) // Добавьте этот флаг в свой Data
                {
                    _data.LastMouseAngle = currentMouseAngle;
                    _data.WasRotatingLastFrame = true;
                }

                float angleDelta = currentMouseAngle - _data.LastMouseAngle;

                // 4. Нормализуем дельту (чтобы при переходе PI -> -PI дельта была 0.01, а не 6.28)
                angleDelta = math.atan2(math.sin(angleDelta), math.cos(angleDelta));
                float deltaDegrees = math.degrees(angleDelta);

                foreach (var snap in _data._selectedEntities)
                {
                    var lt = em.GetComponentData<LocalTransform>(snap.Entity);
                    var rd = em.GetComponentData<RotationData>(snap.Entity);

                    // ВРАЩЕНИЕ ПОЗИЦИИ: теперь крутим текущую позицию на дельту
                    float3 currentOffset = lt.Position - pivot;
                    quaternion stepRotation = quaternion.AxisAngle(new float3(0, 0, 1), angleDelta);
                    float3 rotatedOffset = math.rotate(stepRotation, currentOffset);

                    lt.Position = pivot + rotatedOffset;

                    // ВРАЩЕНИЕ ОБЪЕКТА: просто прибавляем дельту к текущему углу
                    rd.RotateZ += deltaDegrees;
                    // Опционально: держим в пределах 0..360, если это нужно для UI
                    // rd.RotateZ = (rd.RotateZ + 360) % 360;

                    lt.Rotation = GetDegree.FromEuler(new Vector3(0, 0, rd.RotateZ));

                    em.SetComponentData(snap.Entity, lt);
                    em.SetComponentData(snap.Entity, rd);
                }

                _data.LastMouseAngle = currentMouseAngle; // Сохраняем для следующего кадра

                // Важно: вызываем UpdateGroupOBB только для визуальной рамки, 
                // но НЕ меняем GroupCenter, используемый в расчетах выше, до конца драга.
                _updateSquare.UpdateGroupOBB(_selectedEntits, false);
                OnValueChange?.Invoke();
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
            OnValueChange?.Invoke();
        }

        public void EnableTool()
        {
            activeToll = true;
            view.SetActive(true);
        }

        public void DisableTool()
        {
            activeToll = false;
            view.SetActive(false);
        }

        IEnumerator SetEditingStateFalse()
        {
            yield return new WaitForEndOfFrame();
            isEditing = false;
        }

        private WindowsCursorID GetRotatedSizeCursor(WindowsCursorID baseCursor, float rotationDegrees)
        {
            // Нормализуем угол в диапазон [0, 180)
            float angle = rotationDegrees % 180;
            if (angle < 0) angle += 180;

            // Определяем сектор (шаг 45 градусов с порогом 22.5)
            int sector = 0;
            if (angle >= 22.5f && angle < 67.5f) sector = 1; // 45 градусов
            else if (angle >= 67.5f && angle < 112.5f) sector = 2; // 90 градусов
            else if (angle >= 112.5f && angle < 157.5f) sector = 3; // 135 градусов

            // Обработка для прямых сторон (WE / NS)
            if (baseCursor == WindowsCursorID.SizeWE || baseCursor == WindowsCursorID.SizeNS)
            {
                switch (sector)
                {
                    case 1: // 45 градусов
                        return (baseCursor == WindowsCursorID.SizeNS) ? WindowsCursorID.SizeNWSE : WindowsCursorID.SizeNESW;
                    case 2: // 90 градусов
                        return (baseCursor == WindowsCursorID.SizeWE) ? WindowsCursorID.SizeNS : WindowsCursorID.SizeWE;
                    case 3: // 135 градусов
                        return (baseCursor == WindowsCursorID.SizeNS) ? WindowsCursorID.SizeNESW : WindowsCursorID.SizeNWSE;
                    default: // 0 или 180 градусов
                        return baseCursor;
                }
            }

            // Обработка для углов (NWSE / NESW)
            if (baseCursor == WindowsCursorID.SizeNWSE || baseCursor == WindowsCursorID.SizeNESW)
            {
                switch (sector)
                {
                    case 1: // 45 градусов
                        return (baseCursor == WindowsCursorID.SizeNWSE) ? WindowsCursorID.SizeWE : WindowsCursorID.SizeNS;
                    case 2: // 90 градусов
                        return (baseCursor == WindowsCursorID.SizeNWSE) ? WindowsCursorID.SizeNESW : WindowsCursorID.SizeNWSE;
                    case 3: // 135 градусов
                        return (baseCursor == WindowsCursorID.SizeNWSE) ? WindowsCursorID.SizeNS : WindowsCursorID.SizeWE;
                    default: // 0 или 180 градусов
                        return baseCursor;
                }
            }

            return baseCursor;
        }
    }
}