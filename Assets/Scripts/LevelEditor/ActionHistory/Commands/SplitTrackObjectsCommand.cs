using System.Collections.Generic;
using System.Linq;
using TimeLine.LevelEditor.CutTrackObject;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class SplitTrackObjectsCommand : ICommand
    {
        private List<TrackObjectPacket> _target = new();
        private List<TrackObjectPacket> _newPieces = new();
        private List<string> _ids = new();
        private CutTrackObjectController _cutTrackObjectController;
        private TrackObjectStorage _trackObjectStorage;
        private TrackObjectRemover _trackObjectRemover;
        private ResizeTrackObjectCommand _resizeTrackObjectCommand;

        private readonly string _description;

        public SplitTrackObjectsCommand(
            CutTrackObjectController trackObjectController,
            TrackObjectStorage trackObjectStorage,
            TrackObjectRemover trackObjectRemover,
            List<TrackObjectPacket> target,
            string description)
        {
            _trackObjectRemover = trackObjectRemover;
            _description = description;
            _cutTrackObjectController = trackObjectController;
            _trackObjectStorage = trackObjectStorage;
            _target = target.ToList();
        }

        public string Description() => _description;

        public void Execute()
        {
            var previousSize = _target.Select(x => (
                Duraction: x.components.Data.TimeDurationInTicks,
                startTime: x.components.Data.StartTimeInTicks,
                ReduceRight: x.components.Data.ReducedRight,
                ReduceLeft: x.components.Data.ReduceLeft
            )).ToList();

            _newPieces = _cutTrackObjectController.SplitObjectsCommand(_target);

            var newSize = _target.Select(x => (
                Duraction: x.components.Data.TimeDurationInTicks,
                startTime: x.components.Data.StartTimeInTicks,
                ReduceRight: x.components.Data.ReducedRight,
                ReduceLeft: x.components.Data.ReduceLeft
            )).ToList();

            _ids = _newPieces.Select(x => x.sceneObjectID).ToList();


            for (int i = 0; i < previousSize.Count; i++)
            {
                Debug.Log(previousSize[i].Duraction);
                Debug.Log(newSize[i].Duraction);
            }

            _resizeTrackObjectCommand = new ResizeTrackObjectCommand(_trackObjectStorage, _target.ToList(), previousSize, newSize, "");
        }

        public void Undo()
        {
            RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, _newPieces, _ids);
            _trackObjectRemover.RemoveList(_newPieces);
            _resizeTrackObjectCommand.Undo();
        }
    }
}