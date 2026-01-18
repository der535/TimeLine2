using System.Linq;
using TimeLine.CustomInspector.Logic.Parameter;


namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class FloatParameterChangeCommand : ICommand
    {
        private readonly FloatParameter _floatParameter;
        private readonly float _valueBeforeChange;
        private readonly float _valueAfterChange;
        private readonly string _objectId;
        private readonly string _description;
        private readonly TrackObjectStorage _objectStorage;

        public FloatParameterChangeCommand(TrackObjectStorage objectStorage, FloatParameter floatParameter, string description,
            string objectID, float valueBeforeChange, float valueAfterChange)
        {
            _floatParameter = floatParameter;
            _description = description;
            _valueBeforeChange = valueBeforeChange;
            _valueAfterChange = valueAfterChange;
            _objectId = objectID;
            _objectStorage = objectStorage;
        }
        
        public string Description() => _description;

        public void Execute()
        {
            _floatParameter.Value = _valueAfterChange;
        }

        public void Undo()
        {
            if (_floatParameter != null)
                _floatParameter.Value = _valueBeforeChange;
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