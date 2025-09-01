using UnityEngine;

namespace TimeLine
{
    public class GridScene : MonoBehaviour
    {
        [SerializeField] private float _gridSize = 1f;
        [SerializeField] private float _rotateStep = 90f;

        public Vector2 PositionFloatSnapToGrid(Vector2 value, Quaternion rotation)
        {
            // Переводим значение в глобальную систему координат
            Vector3 globalValue = rotation * new Vector3(value.x, value.y, 0);
            
            // Снаппим к сетке в глобальных координатах
            globalValue.x = SnapToGrid(globalValue.x);
            globalValue.y = SnapToGrid(globalValue.y);

            // Возвращаем в локальные координаты
            return (Quaternion.Inverse(rotation) * globalValue);
        }
        
        public float RotateSnapToGrid(float value)
        {
            return Mathf.Round(value / _rotateStep) * _rotateStep;
        }

        public float SnapToGrid(float value)
        {
            if (_gridSize <= 0) return value;
            return Mathf.Round(value / _gridSize) * _gridSize;
        }
    }
}