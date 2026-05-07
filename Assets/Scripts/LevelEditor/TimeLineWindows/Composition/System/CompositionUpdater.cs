using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using TimeLine.Installers;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using TimeLine.LevelEditor.ValueEditor;
using TimeLine.LevelEditor.ValueEditor.Save;
using TimeLine.LevelEditor.ValueEditor.Test;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class CompositionUpdater : MonoBehaviour
    {
        [SerializeField] private TrackObjectStorage trackObjectStorage;

        [FormerlySerializedAs("objectSpawner")] [FormerlySerializedAs("trackObjectSpawner")] [SerializeField]
        private FacadeObjectSpawner facadeObjectSpawner;

        [SerializeField] private SaveComposition composition;
        [SerializeField] private TrackObjectRemover trackObjectRemover;
        [SerializeField] private KeyframeTrackStorage keyframeTrackStorage;
        private LoadGraphLogic _loadGraphLogic;
        private SaveNodes _saveNodes;
        private MainObjects _mainObjects;
        private SaveComposition _saveComposition;

        [Inject]
        private void Construct(MainObjects mainObjects, SaveNodes saveNodes, LoadGraphLogic loadGraphLogic, SaveComposition saveComposition)
        {
            _mainObjects = mainObjects;
            _saveNodes = saveNodes;
            _loadGraphLogic = loadGraphLogic;
            _saveComposition = saveComposition;
        }

        [Button]
        public void UpdateCompositions(string compositionID)
        {
            foreach (var group in trackObjectStorage.TrackObjectGroups.ToList()) //переборка все композиций на сцене
            {
                bool updateSelf = compositionID == group.compositionID; //переключатель обновлять ли себя

                GroupGameObjectSaveData data = composition.FindCompositionDataById(group.compositionID); //ищем обновлённую композицию из хранилища
                List<TrackObjectPacket> trackObjectDatas = new List<TrackObjectPacket>(); //Создаём список объектов
                List<Track> tracks = new List<Track>(); //Создаём список треков что бы у них потом обновить логику в ключевых кадрах

                foreach (var child in data.children) //Перебирает детей из обновлённой композиции взятой их хранилища
                {
                    if (child is GroupGameObjectSaveData groupChild) //Если ребёнок тоже композиция
                    {

                        // if ()
                        // {
                        //     Debug.Log(_saveComposition.TestCheckAvailabilityComposition(group.compositionID,  groupChild.compositionID));
                        //     continue;
                        // }
                        
                        if (!_saveComposition.TestCheckAvailabilityComposition(group.compositionID,  groupChild.compositionID) && updateSelf == false && compositionID != groupChild.compositionID) //Если это не целевая композиция и обновлять себя не надо то скип
                        {
                            continue;
                        }
                        
                        // Debug.Log(_saveComposition.TestCheckAvailabilityComposition(group.compositionID,  groupChild.compositionID));
                       

                        GroupGameObjectSaveData groupChildData =
                            composition.FindCompositionDataById(groupChild.compositionID);
                        if (groupChildData != null)
                        {
                            var (trackData, _, _) = facadeObjectSpawner.LoadComposition(groupChild,
                                groupChild.compositionID,
                                false, groupChildData, false, lastEditID: data.lastEditID);
                            trackObjectDatas.Add(trackData);
                        }
                    }
                    else
                    {
                        if (updateSelf)
                        {
                            var (trackData,  _, tra) = facadeObjectSpawner.LoadObject(child, false);
                            tracks.AddRange(tra);
                            trackObjectDatas.Add(trackData);
                        }
                    }
                }

                foreach (var track in tracks)
                {
                    foreach (var keyframe in track.Keyframes)
                    {
                        if (keyframe.GetEntityData().Graph != null)
                        {
                            keyframe.GetEntityData().Logic = _saveNodes.LoadLogicOnly(keyframe.GetEntityData().Graph,
                                DataType.Color,
                                keyframe.GetEntityData().initializedNodes, trackObjectDatas).Item1;
                        }
                    }
                }

                group.Update(data.duractionTime, trackObjectDatas, trackObjectRemover, _mainObjects,
                    keyframeTrackStorage, data.lastEditID, composition, compositionID, updateSelf, _loadGraphLogic);
            }
        }
    }
}