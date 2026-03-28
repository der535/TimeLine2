using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Newtonsoft.Json;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.ActionHistory;
using TimeLine.LevelEditor.Save;
using TimeLine.TimeLine;
using UnityEngine;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning
{
    public class TrackObjectClipboard
    {
        private List<GameObjectSaveData> dataCopy = new();

        private SaveLevel _saveLevel;
        private Main _main;
        private SaveComposition _saveComposition;
        private ObjectFactory _objectFactory;
        private ObjectLoader _objectLoader;
        private SelectObjectController _selectObjectController;


        public TrackObjectClipboard(
            SaveLevel saveLevel,
            Main main,
            SaveComposition saveComposition,
            ObjectFactory objectFactory,
            ObjectLoader objectLoader,
            SelectObjectController selectObjectController)
        {
            _saveLevel = saveLevel;
            _main = main;
            _saveComposition = saveComposition;
            _objectFactory = objectFactory;
            _objectLoader = objectLoader;
            _selectObjectController = selectObjectController;
        }

        internal void CopyObjects(List<TrackObjectPacket> selectedObjects)
        {
            dataCopy = new List<GameObjectSaveData>();
            var minTicks = selectedObjects.Min(x => x.components.Data.StartTimeInTicks);
            var savedTime = TimeLineConverter.Instance.TicksCurrentTime();
            _main.SetTimeInTicks(minTicks);
            foreach (var sObject in selectedObjects)
            {
                if (sObject is TrackObjectGroup group)
                {
                    GroupGameObjectSaveData saveData = _saveLevel.FullSave(group);
                    saveData.compositionID = group.compositionID;
                    dataCopy.Add(saveData);
                }
                else
                {
                    var data = _saveLevel.SaveGameObject(sObject, "");
                    dataCopy.Add(data);
                }
            }

            _main.SetTimeInTicks(savedTime);
        }

        internal void PasteObjects()
        {
            CommandHistory.IsRecording = false;
            if (dataCopy == null) return;
            var minTime = GetMinTime(dataCopy);

            List<TrackObjectPacket> pastedObjects = new();

            foreach (var data in dataCopy)
            {
                data.startTime = data.startTime - minTime + TimeLineConverter.Instance.TicksCurrentTime();
                if (data is GroupGameObjectSaveData group)
                {
                    pastedObjects.Add(PasteGroup(group, addToTitleCloneText: true));
                }
                else
                {
                    pastedObjects.Add(_objectLoader.LoadObject(data, generateNewSceneID: true, addToTitleCloneText: true).Item1);
                }
            }

            _selectObjectController.DeselectAll();
            _selectObjectController.SelectMultiple(pastedObjects);

            CommandHistory.IsRecording = true;
        }

        private TrackObjectPacket PasteGroup(GroupGameObjectSaveData data, bool addToStorage = true,
            bool addToTitleCloneText = false)
        {
            foreach (var child in data.children)
            {
                if (child is GroupGameObjectSaveData childGroup)
                {
                    PasteGroup(childGroup, false, addToTitleCloneText);
                }
            }

            GroupGameObjectSaveData composition = _saveComposition.FindCompositionDataById(data.compositionID);
            GroupGameObjectSaveData dataCopyComposition = data.DuplicateComposition();

            dataCopyComposition.startTime = 0;
            dataCopyComposition.branch.Nodes = new List<TreeNodeSaveData>();
            dataCopyComposition.tracks = new List<TrackSaveData>();
            dataCopyComposition.EntityComponents = new();


            if (!string.IsNullOrEmpty(data.lastEditID) || !string.IsNullOrEmpty(composition.lastEditID))
            {
                if (data.lastEditID != composition.lastEditID)
                {
                    Debug.Log(composition.compositionID);
                    Debug.Log(composition.lastEditID);
                    Debug.Log(data.lastEditID);
                    data.compositionID = Guid.NewGuid().ToString();
                    dataCopyComposition.compositionID = data.compositionID;
                    _saveComposition.AddComposition(dataCopyComposition);
                }
            }


            if (addToStorage)
                return _objectLoader.LoadComposition(data, data.compositionID, generateNewSceneID: true,
                    addToTitleCloneText: addToTitleCloneText).Item1;
            return null;
        }

        internal bool PasteValidCheck(string past)
        {
            foreach (var copyTrackObject in dataCopy)
            {
                if (copyTrackObject is GroupGameObjectSaveData copyGroup)
                {
                    if (copyGroup.compositionID == past) return false;
                    else if (PasteValidCheckGroup(copyGroup, past)) return false;
                }
            }

            return true;
        }

        internal bool PasteValidCheckGroup(GroupGameObjectSaveData copyGroup, string past)
        {
            foreach (var group in copyGroup.children)
            {
                if (group is GroupGameObjectSaveData groupGroup)
                {
                    if (groupGroup.compositionID == past) return true;
                    else if (PasteValidCheckGroup(groupGroup, past)) return true;
                }
            }

            return false;
        }

        internal double GetMinTime(List<GameObjectSaveData> list)
        {
            return list.Min(item => item.startTime);
        }
    }
}