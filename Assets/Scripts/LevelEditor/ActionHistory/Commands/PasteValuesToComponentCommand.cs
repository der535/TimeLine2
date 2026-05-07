using System.Collections.Generic;
using System.Linq;
using EventBus;
using NUnit.Framework;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.CopyComponent;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using TimeLine.Parent;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class PasteValuesToComponentCommand : ICommand
    {
        private ComponentNames _componentName;
        private TrackObjectPacket trackObjectPacket;
        private CopyComponentController _copyComponentController;
        private GameEventBus _gameEventBus;
        private TrackObjectStorage _trackObjectStorage;
        private string _objectId;

        private ComponentNames _componentNames;
        private Dictionary<string, object> _copyParemetersData;
        private List<(string, Track)> _copyTrack;

        private readonly string _description;

        public PasteValuesToComponentCommand(CopyComponentController copyComponentController, ComponentNames componentName, Entity entity, GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage, string description)
        {
            _trackObjectStorage = trackObjectStorage;
            trackObjectPacket = _trackObjectStorage.GetTrackObjectData(entity);
            _objectId = trackObjectPacket.sceneObjectID;
            _copyComponentController = copyComponentController;
            _componentName = componentName;
            _description = description;
            _gameEventBus = gameEventBus;
        }

        public string Description() => _description;

        public void Execute()
        {
            (_componentNames, _copyParemetersData, _copyTrack) = _copyComponentController.CopyReturn(_componentName, trackObjectPacket.entity);
            _copyComponentController.PasteValues(_componentName, trackObjectPacket.entity);
        }

        public void Undo()
        {
            trackObjectPacket = RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, trackObjectPacket, _objectId);
            _gameEventBus.Raise(new RemoveComponentEvent(_trackObjectStorage.GetTrackObjectData(trackObjectPacket.entity), _componentName));
            _copyComponentController.PasteNewComponent(trackObjectPacket.entity, _componentNames, _copyParemetersData);
            _copyComponentController.PasteValues(_componentName, trackObjectPacket.entity, _copyParemetersData, _copyTrack);
        }
    }
}