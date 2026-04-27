using System.Collections.Generic;
using System.Linq;
using TimeLine.EventBus.Events.TrackObject;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class MoveTrackObjectCommand : ICommand
    {
        private readonly List<TrackObjectPacket> _trackObjects;
        private readonly List<string> _ids;
        private readonly List<(double, int)> _previousPositions, _newPositions;
        private readonly TrackObjectStorage _trackObjectStorage;

        private readonly string _description;

        public MoveTrackObjectCommand(TrackObjectStorage trackObjectStorage, List<TrackObjectPacket> trackObjects, List<(double, int)> previousPositions, List<(double, int)> newPositions, string description)
        {
            _trackObjectStorage = trackObjectStorage;
            _trackObjects = trackObjects;
            _description = description;
            _previousPositions = previousPositions;
            _newPositions = newPositions;
            _ids = _trackObjects.Select(x => x.sceneObjectID).ToList();
        }

        public string Description() => _description;

        public void Execute()
        {
            for (int i = 0; i < _trackObjects.Count; i++)
                _trackObjects[i].components.TrackObject.Move(_newPositions[i].Item1, _newPositions[i].Item2);
        }

        public void Undo()
        {
            RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, _trackObjects, _ids);

            for (int i = 0; i < _trackObjects.Count; i++)
                _trackObjects[i].components.TrackObject.Move(_previousPositions[i].Item1, _previousPositions[i].Item2);
        }
    }
}