using System.Collections.Generic;
using System.Linq;
using EventBus;
using NUnit.Framework;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.EventBus.Events.TrackObject;
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
    public class PastComponentCommand : ICommand
    {
        private ComponentNames _componentName;
        private TrackObjectPacket trackObjectPacket;
        private CopyComponentController _copyComponentController;
        private GameEventBus _gameEventBus;
        private TrackObjectStorage _trackObjectStorage;
        private string _objectId;

        private readonly string _description;

        public PastComponentCommand(CopyComponentController copyComponentController, ComponentNames componentName, Entity entity, GameEventBus gameEventBus, TrackObjectStorage trackObjectStorage, string description)
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
            _copyComponentController.PasteNewComponent(trackObjectPacket.entity);
        }

        public void Undo()
        {
            trackObjectPacket = RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, trackObjectPacket, _objectId);
            _gameEventBus.Raise(new RemoveComponentEvent(_trackObjectStorage.GetTrackObjectData(trackObjectPacket.entity), _componentName));
        }
    }
}