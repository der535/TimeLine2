using UnityEngine;

namespace TimeLine
{
    public class GridScene : MonoBehaviour
    {
        [SerializeField] private float _gridSize = 1000;
        [SerializeField] private float _rotate = 360;

        public Vector2 PositionFloatSnapToGrid(Vector2 value, Quaternion localEulerAngles)
        {
            Vector2 currentValue = value * 1000;
            currentValue =
                new Vector2
                (Mathf.Round(currentValue.x / _gridSize),
                    Mathf.Round(currentValue.y / _gridSize)) * _gridSize;

            // Преобразуем движение обратно в глобальное пространство
            Vector2 globalMovement =  localEulerAngles * currentValue;

            return globalMovement / 1000;
        }

        public float ScaleSnapToGrid(float value)
        {
            float calculatedScale = value;
            print(calculatedScale);
            calculatedScale *= 1000;
            print(calculatedScale);
            calculatedScale = Mathf.Round(calculatedScale/_gridSize)*_gridSize;
            print(calculatedScale);
            print(calculatedScale/1000);
            return calculatedScale/1000;
        }
        
        public float RotateSnapToGrid(float value)
        {
            float calculatedScale = value;
            calculatedScale = Mathf.Round(calculatedScale/_rotate)*_rotate;
            return calculatedScale;
        }
    }
}