using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class GridScene : MonoBehaviour
    {
        [SerializeField] private float _gridSize = 1f;
        [SerializeField] private float _rotateStep = 90f;
        [Space]
        [SerializeField] private TMP_InputField gridSizeInputField;
        [SerializeField] private TMP_InputField gridRotateSizeInputField;

        private void Start()
        {
            gridSizeInputField.text = _gridSize.ToString(CultureInfo.InvariantCulture);
            gridRotateSizeInputField.text = _rotateStep.ToString(CultureInfo.InvariantCulture);
            
            gridSizeInputField.onValueChanged.AddListener(arg0 => 
            {
                if (float.TryParse(arg0, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                {
                    _gridSize = result;
                }
            });

            gridRotateSizeInputField.onValueChanged.AddListener(arg0 => 
            {
                if (float.TryParse(arg0, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                {
                    _rotateStep = result;
                }
            });
        }

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