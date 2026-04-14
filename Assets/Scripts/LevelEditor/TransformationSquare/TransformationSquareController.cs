using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.Cursor;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.Core;
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
        private List<Entity> _selectedEntits;

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
            _mouseDistanceCheck = new TransformationSquareMouseDistanceCheck(_mousePosition, _data, _sceneToRawImageConverter);
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

            _actionMap.Editor.MouseLeft.started += context => { _mouseClick.Click(_selectedEntits); };

            _actionMap.Editor.MouseLeft.canceled += _ =>
            {
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
                ReziseObject(_data._initialBoxLocalMax.x, _data._initialBoxLocalMin.y, false, true);
            }

            if (_data.IsResizingRight && _data.IsResizingUp)
            {
                ReziseObject(_data._initialBoxLocalMin.x, _data._initialBoxLocalMin.y, true, true);
            }

            if (_data.IsResizingRight && _data.IsResizingDown)
            {
                ReziseObject(_data._initialBoxLocalMin.x, _data._initialBoxLocalMax.y, true, false);
            }

            if (_data.IsResizingLeft && _data.IsResizingDown)
            {
                ReziseObject(_data._initialBoxLocalMax.x, _data._initialBoxLocalMax.y, false, false);
            }

            // В методе Update, когда считаете масштаб для правой стороны:
            if (_data.IsResizingRight && !_data.IsResizingDown && !_data.IsResizingUp)
            {
                ReziseObject(_data._initialBoxLocalMin.x, 0, true, true, applyScaleY: false);
            }

            if (_data.IsResizingLeft && !_data.IsResizingDown && !_data.IsResizingUp)
            {
                ReziseObject(_data._initialBoxLocalMax.x, 0, false, true, applyScaleY: false);
            }

            if (_data.IsResizingUp && !_data.IsResizingLeft && !_data.IsResizingRight)
            {
                ReziseObject(0, _data._initialBoxLocalMin.y, true, true, applyScaleX: false);
            }

            if (_data.IsResizingDown && !_data.IsResizingLeft && !_data.IsResizingRight)
            {
                ReziseObject(0, _data._initialBoxLocalMax.y, true, false, applyScaleX: false);
            }

            // В конце метода Click после всех проверок на Resize и Rotate:
            if (_data.IsDragging)
            {
                EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

                // 1. Получаем текущую позицию мыши в мире
                float3 currentMouseWorld = _sceneToRawImageConverter.GetWorldPositionFromMouseOnRawImage();

                // 2. Считаем дельту (разницу) между текущей мышью и той, что была при клике
                // ВАЖНО: Мышь при клике (_data.LastMousePosition) тоже должна быть в мировых координатах
                float3 moveDelta = currentMouseWorld -
                                   new float3(_data.LastMousePosition.x, _data.LastMousePosition.y, 0);

                foreach (var snap in _data._selectedEntities)
                {
                    // 3. Новая позиция = Исходная мировая позиция объекта + дельта мыши
                    // Для этого добавьте в SelectedEntityData поле InitialWorldPos
                    float3 newWorldPos = snap.InitialWorldPos + moveDelta;

                    var lt = em.GetComponentData<LocalTransform>(snap.Entity);
                    lt.Position = newWorldPos;
                    em.SetComponentData(snap.Entity, lt);
                }

                // 4. Обновляем центр группы и рамку, чтобы она ехала за объектами
                _data._groupCenter = _data._groupCenter + moveDelta;
                _updateSquare.UpdateGroupOBB(_selectedEntits, false);
            }

            if (_data.IsRotating)
            {
                EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
                float3 mouseWorld = _sceneToRawImageConverter.GetWorldPositionFromMouseOnRawImage();

                // 1. Считаем угол относительно ТОГО ЖЕ центра, что и при клике
                float currentMouseAngle =
                    math.atan2(mouseWorld.y - _data._groupCenter.y, mouseWorld.x - _data._groupCenter.x);
                float angleDelta = currentMouseAngle - _data._initialMouseAngle;
                quaternion rotationOffset = quaternion.AxisAngle(new float3(0, 0, 1), angleDelta);

                foreach (var snap in _data._selectedEntities)
                {
                    // 2. Вращаем сохраненный "рычаг" (офсет)
                    float3 rotatedOffset = math.rotate(rotationOffset, snap.RotationOffsetWorld);

                    // 3. Новая позиция = Центр + повернутый рычаг
                    float3 newWorldPos = _data._groupCenter + rotatedOffset;

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
                    scaleX = (_data._initialBoxSize.x + deltaX) / _data._initialBoxSize.x;
                else
                    scaleX = (_data._initialBoxSize.x - deltaX) / _data._initialBoxSize.x;

                scaleX = math.max(scaleX, 0.01f);
            }

            if (applyScaleY)
            {
                if (scaleYPlusDelta)
                    scaleY = (_data._initialBoxSize.y + deltaY) / _data._initialBoxSize.y;
                else
                    scaleY = (_data._initialBoxSize.y - deltaY) / _data._initialBoxSize.y;

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