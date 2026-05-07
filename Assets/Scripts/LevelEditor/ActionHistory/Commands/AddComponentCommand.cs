using System.Collections.Generic;
using System.Linq;
using EventBus;
using Newtonsoft.Json;
using NUnit.Framework;
using TimeLine.Components;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.CopyComponent;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class AddComponentCommand : ICommand
    {
        private readonly EntityComponentController _controller;
        private readonly GameEventBus _eventBus;
        private readonly AddComponentWindowsController _addComponentWindowsController;
        private readonly TrackObjectStorage _trackObjectStorage;
        private readonly KeyframeTrackStorage _keyframeTrackStorage;

        private ComponentNames _componentNames;
        private TrackObjectPacket _trackObjectPacket;
        private string _savedID;

        private readonly string _description;

        public AddComponentCommand(
            EntityComponentController entityComponentController,
            GameEventBus gameEventBus,
            AddComponentWindowsController addComponentWindowsController,
            TrackObjectStorage trackObjectStorage,
            KeyframeTrackStorage keyframeTrackStorage,
            ComponentNames componentNames,
            Entity entity,
            string description)
        {
            _description = description;
            _controller = entityComponentController;
            _eventBus = gameEventBus;
            _trackObjectStorage = trackObjectStorage;
            _keyframeTrackStorage = keyframeTrackStorage;

            _componentNames = componentNames;
            _trackObjectPacket = _trackObjectStorage.GetTrackObjectData(entity);

            _addComponentWindowsController = addComponentWindowsController;
        }

        public string Description() => _description;

        public void Execute()
        {
            _controller.AddComponentSafely(_componentNames, _trackObjectPacket.entity);
            _eventBus.Raise(new AddComponentEvent(_trackObjectStorage.GetTrackObjectData(_trackObjectPacket.entity),
                _componentNames, _trackObjectPacket.entity));
            _addComponentWindowsController.UpdateComponents(_trackObjectPacket.entity);
        }

        public void Undo()
        {
            _trackObjectPacket = RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, _trackObjectPacket, _savedID);
            _controller.RemoveComponent(_componentNames, _keyframeTrackStorage, _trackObjectPacket.entity);
        }
    }
}