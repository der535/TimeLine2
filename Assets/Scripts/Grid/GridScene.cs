using UnityEngine;

namespace TimeLine
{
    public class GridScene : MonoBehaviour
    {
        [SerializeField] private float _gridSize = 1000;

        public Vector2 FloatSnapToGrid(Vector2 value, Quaternion localEulerAngles)
        {

            Vector2 currentValue =  value * 1000;
            currentValue = new Vector2(Mathf.Round(currentValue.x / _gridSize),Mathf.Round(currentValue.y / _gridSize)) * _gridSize;
            print($"currentValue: {value} / {currentValue}");
            
            // Преобразуем движение обратно в глобальное пространство
            Vector2 globalMovement = currentValue;
            
            
            return globalMovement/1000;
        }
    }
}