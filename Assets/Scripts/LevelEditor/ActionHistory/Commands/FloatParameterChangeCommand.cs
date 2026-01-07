using TimeLine.CustomInspector.Logic.Parameter;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class FloatParameterChangeCommand : ICommand
    {
        private FloatParameter _floatParameter;
        private float _valueBeforeChange, _valueAfterChange;
        private string _objectId;
        private TrackObjectStorage _objectStorage;
        
        public FloatParameterChangeCommand(TrackObjectStorage objectStorage, FloatParameter floatParameter, string objectID,  float valueBeforeChange, float valueAfterChange)
        {
            _floatParameter = floatParameter;
            
            _valueBeforeChange = valueBeforeChange;
            _valueAfterChange = valueAfterChange;
            _objectId = objectID;
        }
        public void Execute()
        {
            _floatParameter.Value = _valueAfterChange;
        }

        public void Undo()
        {
            if(_floatParameter != null)
                _floatParameter.Value = _valueBeforeChange;
            else
            {
                _objectStorage.FindObjectByID(_objectId);
            }
        }
    }
}