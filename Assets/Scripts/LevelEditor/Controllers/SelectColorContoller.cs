using System;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.ActionHistory;
using TimeLine.LevelEditor.ActionHistory.Commands;
using UnityEngine;

namespace TimeLine
{
    public class SelectColorContoller : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private FlexibleColorPicker flexibleColorPicker;

        private ColorParameter _colorParameter;
        private TrackObjectStorage _trackObjectStorage;
        private Color _previousValue;
        private string _gameObjectID;

        private void Construct(TrackObjectStorage trackObjectStorage)
        {
            _trackObjectStorage = trackObjectStorage;
        }
        private void Start()
        {
            flexibleColorPicker.onColorChange.AddListener((value) =>
            {
                CommandHistory.ExecuteCommand(new ColorParameterChangeCommand(_trackObjectStorage, _colorParameter,
                    _colorParameter.Name, _gameObjectID, _previousValue, value));
                _previousValue = _colorParameter.Value;
                //
                // _colorParameter.Value = color;
            }); 
        }

        internal void Setup(ColorParameter colorParameter, string gameObjectId)
        {
            _gameObjectID = gameObjectId;
            _colorParameter = colorParameter;
            rectTransform.gameObject.SetActive(true);
            flexibleColorPicker.color = colorParameter.Value;
        }

        public void Close()
        {
            rectTransform.gameObject.SetActive(false);
        }

        public void Select()
        {
            _colorParameter.Value = flexibleColorPicker.color;
            Close();
        }
    }
}
