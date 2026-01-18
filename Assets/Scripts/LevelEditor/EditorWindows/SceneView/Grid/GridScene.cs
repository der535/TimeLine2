using System;
using System.Globalization;
using System.IO;
using EventBus;
using EventBus.Events.Settings;
using TimeLine.EventBus.Events.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class GridScene : MonoBehaviour
    {
        [FormerlySerializedAs("_gridSize")] 
        [SerializeField] private float _positionStepSize = 1f;
        [SerializeField] private float _rotateStep = 90f;
        
        [Space] 
        [SerializeField] private TMP_InputField gridSizeInputField;
        [SerializeField] private TMP_InputField gridRotateSizeInputField;

        private GameEventBus _gameEventBus;
        private Vector2 _gridOffset = Vector2.zero;
        private string _fullLogPath;
        private StreamWriter _logWriter;
        private TimeLineSettings _timeLineSettings;

        private bool load;

        [Inject]
        private void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }

        internal (float, float) GetGridSize()
        {
            return (float.Parse(gridSizeInputField.text, CultureInfo.InvariantCulture), 
                    float.Parse(gridRotateSizeInputField.text, CultureInfo.InvariantCulture));
        }

        internal void SetGridSize(float gridSize, float rotateGridSize)
        {
            load = true;
            gridSizeInputField.text = gridSize.ToString(CultureInfo.InvariantCulture);
            gridRotateSizeInputField.text = rotateGridSize.ToString(CultureInfo.InvariantCulture);
            gridSizeInputField.onValueChanged.Invoke(gridSizeInputField.text);
            gridRotateSizeInputField.onValueChanged.Invoke(gridRotateSizeInputField.text);
            load = false;
        }

        private void Start()
        {
            gridSizeInputField.text = _positionStepSize.ToString(CultureInfo.InvariantCulture);
            gridRotateSizeInputField.text = _rotateStep.ToString(CultureInfo.InvariantCulture);

            FloatInputValidator gridSizeInputFieldValidator = new FloatInputValidator(gridSizeInputField, f =>
            {
                SetPositionStepSize(f);
                if(!load) _gameEventBus.Raise(new ChangeEditorSettingsEvent());
            }, 0);
            
            FloatInputValidator gridRotateSizeInputFieldValidator = new FloatInputValidator(gridRotateSizeInputField, f =>
            {
                _rotateStep = f;
                if(!load) _gameEventBus.Raise(new ChangeEditorSettingsEvent());
            }, 0);
        }

        
        public void SetCurrentObjectPosition(Vector2 position)
        {
            if (_positionStepSize <= 0f)
            {
                _gridOffset = Vector2.zero;
                return;
            }
            
            float snappedX = SnapToGrid(position.x);
            float snappedY = SnapToGrid(position.y);
            _gridOffset = position - new Vector2(snappedX, snappedY);
        }

        public void SetPositionStepSize(float newStepSize)
        {
            if (newStepSize <= 0 || Mathf.Approximately(newStepSize, _positionStepSize)) return;

            _positionStepSize = newStepSize;
            _gameEventBus?.Raise(new GridPositionEvent(newStepSize));
        }

        public Vector2 PositionFloatSnapToGrid(Vector2 value, Quaternion inverseRotation)
        {
            // Перевод в локальные координаты
            Vector3 localValue = inverseRotation * new Vector3(value.x, value.y, 0);
            
            // Снаппинг с оффсетом
            Vector2 snappedLocal = SnapToGridWithOffset(new Vector2(localValue.x, localValue.y), _positionStepSize, _gridOffset);

            // Обратно в мировые
            Vector3 snappedWorld = Quaternion.Inverse(inverseRotation) * new Vector3(snappedLocal.x, snappedLocal.y, 0);
            return new Vector2(snappedWorld.x, snappedWorld.y);
        }

        public float RotateSnapToGrid(float value)
        {
            if(_rotateStep <= 0) return value;
            return Mathf.Round(value / _rotateStep) * _rotateStep;
        }

        public float SnapToGrid(float value)
        {
            if (_positionStepSize <= 0) return value;
            return Mathf.Round(value / _positionStepSize) * _positionStepSize;
        }

        private Vector2 SnapToGridWithOffset(Vector2 position, float step, Vector2 offset)
        {
            if (step <= 0f) return position;

            Vector2 adjusted = position - offset;
            Vector2 snapped = new Vector2(
                Mathf.Round(adjusted.x / step) * step,
                Mathf.Round(adjusted.y / step) * step
            );

            return snapped + offset;
        }

        private void OnDestroy()
        {
            if (_logWriter != null)
            {
                _logWriter.Close();
                _logWriter = null;
            }
        }

        private void OnApplicationQuit()
        {
            if (_logWriter != null)
            {
                _logWriter.Close();
                _logWriter = null;
            }
        }
    }
}