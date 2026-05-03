using System.Collections.Generic;
using System.Linq;
using EventBus;
using Newtonsoft.Json;
using NUnit.Framework;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.CopyComponent;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class RemoveComponentCommand : ICommand
    {
        private ComponentNames _componentName;
        private TrackObjectPacket trackObjectPacket;
        private CopyComponentController _copyComponentController;
        private GameEventBus _gameEventBus;
        private TrackObjectStorage _trackObjectStorage;
        private string _objectId;

        private ComponentNames savedComponedNames;
        private Dictionary<string, object> savedParemetersDatas;
        private List<(string, Track)> savedTracks;

        private readonly string _description;

        public RemoveComponentCommand(CopyComponentController copyComponentController, ComponentNames componentName, Entity entity, GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage, string description)
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
            (savedComponedNames, savedParemetersDatas, savedTracks) = _copyComponentController.CopyReturn(_componentName, trackObjectPacket.entity);
            _gameEventBus.Raise(new RemoveComponentEvent(_trackObjectStorage.GetTrackObjectData(trackObjectPacket.entity), _componentName));
        }

        public void Undo()
        {
            trackObjectPacket = RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, trackObjectPacket, _objectId);
            _copyComponentController.PasteNewComponent(trackObjectPacket.entity, savedComponedNames, savedParemetersDatas);
            _copyComponentController.PasteValues(savedComponedNames, trackObjectPacket.entity, savedParemetersDatas, savedTracks);
        }
    }
}