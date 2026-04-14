using Unity.Mathematics;
using UnityEngine;

namespace TimeLine.LevelEditor.TransformationSquare.Service
{
    public class TransformationSquareMouseDistanceCheck
    {
        private TransformationSquareMousePosition _mousePosition;
        private SceneToRawImageConverter _sceneToRawImageConverter;
        private TransformationSquareData _data;

        public TransformationSquareMouseDistanceCheck(TransformationSquareMousePosition mousePosition,
            TransformationSquareData data, SceneToRawImageConverter sceneToRawImageConverter)
        {
            _mousePosition = mousePosition;
            _data = data;
            _sceneToRawImageConverter = sceneToRawImageConverter;
        }

        private float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            float l2 = Vector2.SqrMagnitude(a - b);
            if (l2 == 0.0f) return Vector2.Distance(p, a);

            // Проекция точки p на прямую ab, ограниченная отрезком [0, 1]
            float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(p - a, b - a) / l2));
            Vector2 projection = a + t * (b - a);
            return Vector2.Distance(p, projection);
        }


        public bool IsMouseInsideBox()
        {
            
            
            float3 mouseLocal = math.transform(_data.WorldToPivotMatrix,
                new float3(_sceneToRawImageConverter.GetWorldPositionFromMouseOnRawImage().x, _sceneToRawImageConverter.GetWorldPositionFromMouseOnRawImage().y, 0f));
            return mouseLocal.x >= _data.CurrentLocalMin.x && mouseLocal.x <= _data.CurrentLocalMax.x &&
                   mouseLocal.y >= _data.CurrentLocalMin.y && mouseLocal.y <= _data.CurrentLocalMax.y;
        }

        public bool RightLine()
        {
            // Линия между TopRight (1) и BottomRight (2)
            return DistanceToSegment(_mousePosition.Get(), _data.UIPoints[1], _data.UIPoints[2]) <=
                   TransformationSquareData.DistanceToResize;
        }

        public bool LeftLine()
        {
            // Линия между TopLeft (0) и BottomLeft (3)
            return DistanceToSegment(_mousePosition.Get(), _data.UIPoints[0], _data.UIPoints[3]) <=
                   TransformationSquareData.DistanceToResize;
        }

        public bool UpBorder()
        {
            // Линия между TopLeft (0) и TopRight (1)
            return DistanceToSegment(_mousePosition.Get(), _data.UIPoints[0], _data.UIPoints[1]) <=
                   TransformationSquareData.DistanceToResize;
        }

        public bool MouseInResizeAreaDown()
        {
            // Линия между BottomLeft (3) и BottomRight (2)
            return DistanceToSegment(_mousePosition.Get(), _data.UIPoints[3], _data.UIPoints[2]) <=
                   TransformationSquareData.DistanceToResize;
        }

        public bool TopLeftCorner()
        {
            // Проверка дистанции до точки _data.UIPoints[2] (BottomRight)
            return Vector2.Distance(_mousePosition.Get(), _data.UIPoints[0]) <=
                   TransformationSquareData.DistanceToResize;
        }

        public bool TopRightCorner()
        {
            // Проверка дистанции до точки _uiPoints[2] (BottomRight)
            return Vector2.Distance(_mousePosition.Get(), _data.UIPoints[1]) <=
                   TransformationSquareData.DistanceToResize;
        }

        public bool BottomRightCorner()
        {
            // Проверка дистанции до точки _data.UIPoints[2] (BottomRight)
            return Vector2.Distance(_mousePosition.Get(), _data.UIPoints[2]) <=
                   TransformationSquareData.DistanceToResize;
        }

        public bool BottomLeftCorner()
        {
            // Проверка дистанции до точки _data.UIPoints[2] (BottomRight)
            return Vector2.Distance(_mousePosition.Get(), _data.UIPoints[3]) <=
                   TransformationSquareData.DistanceToResize;
        }

        public bool CheckMouseAllPointsDistanceToRotate()
        {
            return CheckMouseDistanceToRotate(_data.UIPoints[0]) || CheckMouseDistanceToRotate(_data.UIPoints[1]) ||
                   CheckMouseDistanceToRotate(_data.UIPoints[2]) || CheckMouseDistanceToRotate(_data.UIPoints[3]);
        }

        public bool CheckMouseDistanceToRotate(Vector2 checkPoint)
        {
            // Проверка дистанции до точки _uiPoints[2] (BottomRight)
            var distance = Vector2.Distance(_mousePosition.Get(), checkPoint);

            return distance > TransformationSquareData.DistanceToResize &&
                   distance <= TransformationSquareData.DistanceToRotate;
        }
    }
}