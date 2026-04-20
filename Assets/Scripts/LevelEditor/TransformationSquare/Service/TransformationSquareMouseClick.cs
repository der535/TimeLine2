using System.Collections.Generic;
using TimeLine.LevelEditor.ECS.Services;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.LevelEditor.TransformationSquare.Service
{
    /// <summary>
    /// Класс предназначенный для обработки клика мыши и деланий всяких проверок
    /// </summary>
    public class TransformationSquareMouseClick
    {
        private readonly TransformationSquareData _data;
        private readonly SceneToRawImageConverter _sceneToRawImageConverter;
        private readonly TransformationSquareMouseDistanceCheck _distanceCheck;
        
        public TransformationSquareMouseClick(TransformationSquareData data, SceneToRawImageConverter sceneToRawImageConverter, TransformationSquareMouseDistanceCheck distanceCheck)
        {
            _data = data;
            _sceneToRawImageConverter = sceneToRawImageConverter;
            _distanceCheck = distanceCheck;
        }

        private float3 GetGroupCenter(List<Entity> entities)
        {
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            float3 center = float3.zero;

            foreach (var entity in entities)
            {
                center += em.GetComponentData<LocalTransform>(entity).Position;
            }

            return center / entities.Count;
        }

        public void UpdateSelectedEntities(List<Entity> entities)
        {
            _data._selectedEntities.Clear();
            
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            foreach (var entity in entities)
            {
                var lt = entityManager.GetComponentData<LocalTransform>(entity);
                var ptm = entityManager.GetComponentData<PostTransformMatrix>(entity);
        
                _data._selectedEntities.Add(new TransformationSquareData.SelectedEntityData
                {
                    Entity = entity,
                    RotationOffsetWorld = lt.Position - _data.GroupCenter,
                    // Важно: сохраняем позицию объекта в пространстве повернутой рамки
                    LocalPosInBox = math.transform(_data.WorldToPivotMatrix, lt.Position),
                    InitialWorldPos = lt.Position,
                    InitialScale = GetScaleFromMatrix.Get(ptm.Value),
                    InitialRotation = lt.Rotation
                });
            }
        }
        
        public void Click(List<Entity> selectedEntity)
        {
            if( selectedEntity == null) return;
            
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            
            float3 mouseWorld = _sceneToRawImageConverter.GetWorldPositionFromMouseOnRawImage();
            // Угол от центра группы до мышки
            _data.InitialMouseAngle = math.atan2(mouseWorld.y - _data.GroupCenter.y, mouseWorld.x - _data.GroupCenter.x);

            _data.LastMousePosition = _sceneToRawImageConverter.GetWorldPositionFromMouseOnRawImage();
            
            // UpdateSelectedEntities(selectedEntity);
            
            _data.InitialBoxSize = _data.CurrentLocalMax - _data.CurrentLocalMin;
            _data.InitialBoxLocalMin = _data.CurrentLocalMin;
            _data.InitialBoxLocalMax = _data.CurrentLocalMax;
            
            if (_distanceCheck.CheckMouseAllPointsDistanceToRotate())
                _data.IsRotating = true;

            _data.IsResizingUp = _distanceCheck.TopLeftCorner() ||
                                 _distanceCheck.TopRightCorner() ||
                                 _distanceCheck.UpBorder();

            _data.IsResizingLeft = _distanceCheck.TopLeftCorner() ||
                                   _distanceCheck.BottomLeftCorner() ||
                                   _distanceCheck.LeftLine();

            _data.IsResizingRight = _distanceCheck.TopRightCorner() ||
                                    _distanceCheck.BottomRightCorner() ||
                                    _distanceCheck.RightLine();

            _data.IsResizingDown = _distanceCheck.BottomRightCorner() ||
                                   _distanceCheck.BottomLeftCorner() ||
                                   _distanceCheck.MouseInResizeAreaDown();

            bool anyResizeOrRotate = _data.IsResizingLeft || _data.IsResizingRight || 
                                     _data.IsResizingUp || _data.IsResizingDown || _data.IsRotating;
            
            _data.IsDragging = !anyResizeOrRotate && _distanceCheck.IsMouseInsideBox();
        }

    }
}