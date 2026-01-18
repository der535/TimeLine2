using System.Linq;
using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;


namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class ColorParameterChangeCommand : ICommand
    {
        private readonly ColorParameter _colorParameter;
        private readonly Color _valueBeforeChange;
        private readonly Color _valueAfterChange;
        private readonly string _objectId;
        private readonly string _description;
        private readonly TrackObjectStorage _objectStorage;

        public ColorParameterChangeCommand(TrackObjectStorage objectStorage, ColorParameter colorParameter, string description,
            string objectID, Color valueBeforeChange, Color valueAfterChange)
        {
            _colorParameter = colorParameter;
            _description = description;
            _valueBeforeChange = valueBeforeChange;
            _valueAfterChange = valueAfterChange;
            _objectId = objectID;
            _objectStorage = objectStorage;
        }
        
        public string Description() => _description;

        public void Execute()
        {
            _colorParameter.Value = _valueAfterChange;
        }

        public void Undo()
        {
            if (_colorParameter != null)
                _colorParameter.Value = _valueBeforeChange;
            else
            {
                var components = _objectStorage.FindObjectByID(_objectId).sceneObject
                    .GetComponents<BaseParameterComponent>();
                foreach (var component in components)
                {
                    component.GetParameterData().ToList().Find(x => x.Value.Id == _objectId).Value.Value =
                        _valueAfterChange;
                }
            }
        }
    }
}