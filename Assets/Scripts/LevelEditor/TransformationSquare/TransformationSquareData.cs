
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TimeLine.LevelEditor.TransformationSquare
{
    public class TransformationSquareData
    {
        public const float DistanceToResize = 10;
        public const float DistanceToRotate = 20;
        
        public bool IsResizingRight;
        public bool IsResizingLeft;
        public bool IsResizingUp;
        public bool IsResizingDown;
        public bool IsRotating;
        public bool IsDragging;

        public bool GetIsEditingObject()
        {
            return IsResizingRight || IsResizingLeft || IsResizingUp || IsResizingDown || IsRotating || IsDragging;
        }
        
        public float4x4 WorldToPivotMatrix; // Матрица перевода из мира в пространство рамки
        public float4x4 PivotToWorldMatrix; // Матрица обратно

        public Vector2[] UIPoints = new Vector2[4];

        public Vector2 LastMousePosition;
        
        public float3 GroupCenter;
        
        public float InitialMouseAngle;
        
        
        public float3 InitialBoxSize;
        public float3 InitialBoxLocalMin;
        public float3 InitialBoxLocalMax;
        
        public float3 CurrentLocalMin; // Сохраняем вычисленный min
        public float3 CurrentLocalMax; // Сохраняем вычисленный max

        
        public struct SelectedEntityData
        {
            public Entity Entity;
            public float3 LocalPosInBox;      // Для ресайза (в пространстве OBB)
            public float3 RotationOffsetWorld; // Для вращения (вектор ПозицияОбъекта - ЦентрГруппы)
            public float3 InitialWorldPos;
            public float3 InitialScale;
            
            public quaternion InitialRotation;
        }

// При клике (started) сохраняйте данные для всех выделенных объектов
        public List<SelectedEntityData> _selectedEntities = new();
    }
}