using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class PastObjectsCommand : ICommand
    {
        private List<TrackObjectPacket> _pastedObjects = new();
        private List<string> _ids = new();
        private TrackObjectRemover _trackObjectRemover;
        private TrackObjectGroup _group;
        private TrackObjectClipboard _trackObjectClipboard;
        private TrackObjectStorage _trackObjectStorage;
        private List<GameObjectSaveData> _saveObjects;
        
        private readonly string _description;

        public PastObjectsCommand(TrackObjectClipboard trackObjectClipboard, TrackObjectStorage trackObjectStorage, TrackObjectRemover trackObjectRemover, List<GameObjectSaveData> copydata,  string description)
        {
            _trackObjectRemover = trackObjectRemover;
            _trackObjectClipboard = trackObjectClipboard;
            _trackObjectStorage = trackObjectStorage;
            _saveObjects = copydata;
            _description = description;
        }

        public string Description() => _description;

        public void Execute()
        {
            _pastedObjects =  _trackObjectClipboard.PasteObjectsFromSave(_saveObjects, TimeLineConverter.Instance.TicksCurrentTime());
            _ids = _pastedObjects.Select(x => x.sceneObjectID).ToList();
        }

        public void Undo()
        {
            RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, _pastedObjects, _ids);
            _trackObjectRemover.RemoveList(_pastedObjects);
        }
    }
}