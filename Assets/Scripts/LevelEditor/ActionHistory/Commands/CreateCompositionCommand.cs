using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class CreateCompositionCommand : ICommand
    {
        private List<TrackObjectPacket> _target;
        private List<GameObjectSaveData> _saveDatas = new();
        private string _compositionName;

        private TrackObjectGroup _group;
        private string _groupIP;
        private string _compositionID;


        private readonly string _description;
        private readonly TrackObjectStorage _trackObjectStorage;
        private readonly SaveComposition _saveComposition;
        private readonly GroupCreater _groupCreater;
        private readonly FacadeObjectSpawner _facadeObjectSpawner;
        private readonly TrackObjectRemover _trackObjectRemover;


        public CreateCompositionCommand(SaveLevel saveLevel, SaveComposition saveComposition, TrackObjectRemover trackObjectRemover, FacadeObjectSpawner facadeObjectSpawner, TrackObjectStorage trackObjectStorage, GroupCreater groupCreater, string compositionName, List<TrackObjectPacket> target, string description)
        {
            _saveComposition = saveComposition;
            _groupCreater = groupCreater;
            _target = target;
            _description = description;
            _trackObjectRemover = trackObjectRemover;
            _trackObjectStorage = trackObjectStorage;
            _compositionName = compositionName;
            _facadeObjectSpawner = facadeObjectSpawner;

            foreach (var item in target)
            {
                if (item is TrackObjectGroup trackObjectPacket)
                    _saveDatas.Add(saveLevel.SaveGroup(trackObjectPacket, saveGroupID: true));
                else
                    _saveDatas.Add(saveLevel.SaveGameObject(item, string.Empty));
            }
        }

        public string Description() => _description;

        public void Execute()
        {
            _group = _groupCreater.Create(_compositionName, _target);
            _groupIP = _group.sceneObjectID;
            _compositionID = _group.compositionID;
            _target.Clear();
        }

        public void Undo()
        {
            _group = (TrackObjectGroup)RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, _group, _groupIP);

            _trackObjectRemover.ListRemove(_group);
            _target = _facadeObjectSpawner.LoadObjects(_saveDatas);

            bool deleteCompositionFromStorage = true;
            foreach (var group in _trackObjectStorage.TrackObjectGroups)
            {
                if (group.compositionID == _compositionID)
                {
                    deleteCompositionFromStorage = false;
                    break;
                }
            }
            if(deleteCompositionFromStorage) _saveComposition.DeleteComposition(_compositionID);
            
        }
    }
}