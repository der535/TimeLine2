using System.Collections.Generic;
using System.Linq;
using TimeLine.EventBus.Events.TrackObject;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class SelectObjectCommand : ICommand
    {
        private readonly List<TrackObjectPacket> _previousState, _newState;
        private readonly List<string> _previousIds, _newIds;
        private readonly TrackObjectStorage _trackObjectStorage;

        private readonly string _description;

        private readonly SelectObjectController _selectObjectController;

        public SelectObjectCommand(TrackObjectStorage trackObjectStorage, SelectObjectController selectObjectController, List<TrackObjectPacket> previousState, List<TrackObjectPacket> newState, string description)
        {
            _previousState = previousState;
            _newState = newState;
            _description = description;
            _selectObjectController = selectObjectController;
            _trackObjectStorage = trackObjectStorage;
            _previousIds = _previousState.Select(x => x.sceneObjectID).ToList();
            _newIds = newState.Select(x => x.sceneObjectID).ToList();
        }
        
        public string Description() => _description;
        
        public void Execute()
        {
            RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, _newState, _newIds);

            _selectObjectController.SelectMultipleCommand(_newState);
        }

        public void Undo()
        {
            RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, _previousState, _previousIds);

            _selectObjectController.SelectMultipleCommand(_previousState);
        }
    }
}