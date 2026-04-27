using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class ResizeTrackObjectCommand : ICommand
    {
        private readonly List<TrackObjectPacket> _trackObjects;
        private readonly List<string> _ids;
        private readonly List<(double Duraction, double startTime, double ReduceRight, double ReduceLeft)> _previousSize, _newSize;
        private readonly TrackObjectStorage _trackObjectStorage;
        private readonly string _description;

        public ResizeTrackObjectCommand(
            TrackObjectStorage trackObjectStorage,
            List<TrackObjectPacket> trackObjects,
            List<(double Duraction, double startTime, double ReduceRight, double ReduceLeft)> previousSize,
            List<(double Duraction, double startTime, double ReduceRight, double ReduceLeft)> newSize,
            string description)
        {
            _trackObjects = trackObjects;
            _description = description;
            _previousSize = previousSize;
            _newSize = newSize;
            _trackObjectStorage = trackObjectStorage;
            _ids = _trackObjects.Select(x => x.sceneObjectID).ToList();
        }

        public string Description() => _description;

        public void Execute()
        {
            MultipleLeftResize(_newSize);
        }

        public void MultipleLeftResize(List<(double Duraction, double startTime, double ReduceRight, double ReduceLeft)> newSize)
        {
            for (int i = 0; i < _trackObjects.Count; i++)
            {
                _trackObjects[i].components.Data.TimeDurationInTicks = newSize[i].Duraction;
                _trackObjects[i].components.Data.StartTimeInTicks = newSize[i].startTime;
                _trackObjects[i].components.Data.ReducedRight = newSize[i].ReduceRight;
                _trackObjects[i].components.Data.ReduceLeft = newSize[i].ReduceLeft;
                _trackObjects[i].components.TrackObject.UpdateVisuals();
            }
        }

        public void Undo()
        {
            RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, _trackObjects, _ids);

            MultipleLeftResize(_previousSize);
        }
    }
}