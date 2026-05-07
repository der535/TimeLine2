using System.Collections.Generic;
using System.Linq;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class DeleteObjectCommand : ICommand
    {
        private List<TrackObjectPacket> _target;
        private List<GameObjectSaveData> _saveDatas = new();

        private readonly string _description;
        private TrackObjectStorage _trackObjectStorage;
        private SaveLevel _saveLevel;
        private readonly FacadeObjectSpawner _facadeObjectSpawner;
        private TrackObjectRemover _trackObjectRemover;

        public DeleteObjectCommand(SaveLevel save, FacadeObjectSpawner trackObjectStorage, TrackObjectRemover trackObjectRemover, List<TrackObjectPacket> target, string description)
        {
            _target = target;
            _description = description;
            _saveLevel = save;
            _facadeObjectSpawner = trackObjectStorage;
            _trackObjectRemover = trackObjectRemover;

            foreach (var item in target)
            {
                if (item is TrackObjectGroup trackObjectPacket)
                    _saveDatas.Add(save.SaveGroup(trackObjectPacket, saveGroupID: true));
                else
                    _saveDatas.Add(save.SaveGameObject(item, string.Empty));
            }
        }

        public string Description() => _description;

        public void Execute()
        {
            _trackObjectRemover.RemoveList(_target);
            _target.Clear();
        }

        public void Undo()
        {
            _target = _facadeObjectSpawner.LoadObjects(_saveDatas);
        }
    }
}