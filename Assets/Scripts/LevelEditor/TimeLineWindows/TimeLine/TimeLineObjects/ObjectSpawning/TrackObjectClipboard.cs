using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TimeLine.TimeLine;

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
        
        
        public TrackObjectClipboard(
            SaveLevel saveLevel, 
            Main main, 
            SaveComposition saveComposition, 
            ObjectFactory objectFactory,
            ObjectLoader objectLoader)
        {
            _saveLevel = saveLevel;
            _main = main;
            _saveComposition = saveComposition;
            _objectFactory = objectFactory;
            _objectLoader = objectLoader;
        }

        internal void CopyObjects(List<TrackObjectData> selectedObjects)
        {
            dataCopy = new List<GameObjectSaveData>();
            var minTicks = selectedObjects.Min(x => x.trackObject.StartTimeInTicks);
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
            if(dataCopy == null) return;
            var minTime = GetMinTime(dataCopy);
            foreach (var data in dataCopy)
            {
                data.startTime = data.startTime - minTime + TimeLineConverter.Instance.TicksCurrentTime();
                if (data is GroupGameObjectSaveData group)
                {
                    PasteGroup(group, generateNewSceneID: true, addToTitleCloneText: true);
                }
                else
                {
                    _objectLoader.LoadObject(data, generateNewSceneID: true, addToTitleCloneText: true);
                }
            }

            // dataCopy = new List<GameObjectSaveData>();
        }
        
        private void PasteGroup(GroupGameObjectSaveData data, bool addToStorage = true, bool generateNewSceneID = false, bool addToTitleCloneText = false)
        {
            foreach (var child in data.children)
            {
                if (child is GroupGameObjectSaveData childGroup)
                {
                    PasteGroup(childGroup, false, generateNewSceneID, addToTitleCloneText);
                }
            }

            GroupGameObjectSaveData composition = _saveComposition.FindCompositionDataById(data.compositionID);
            GroupGameObjectSaveData dataCopyComposition = data.DuplicateComposition();

            dataCopyComposition.startTime = 0;
            dataCopyComposition.branch.Nodes = new List<TreeNodeSaveData>();
            dataCopyComposition.tracks = new List<TrackSaveData>();
            dataCopyComposition.Components =
                new List<ComponentData>(); // В сохраняемую композицию запихать стандартные компонеты из префаба

            
            if (!string.IsNullOrEmpty(data.lastEditID) || !string.IsNullOrEmpty(composition.lastEditID))
            {
                if (data.lastEditID != composition.lastEditID)
                {
                    data.compositionID = Guid.NewGuid().ToString();
                    dataCopyComposition.compositionID = data.compositionID;
                    _saveComposition.AddComposition(dataCopyComposition);
                }
            }
            


            if (addToStorage)
                _objectLoader.LoadComposition(data, data.compositionID,  generateNewSceneID:generateNewSceneID, addToTitleCloneText:addToTitleCloneText);
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
                    else if(PasteValidCheckGroup(groupGroup, past)) return true;
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