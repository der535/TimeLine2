using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using TimeLine.Installers;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
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

        [Inject]
        private void Construct(MainObjects mainObjects, SaveNodes saveNodes, LoadGraphLogic loadGraphLogic)
        {
            _mainObjects = mainObjects;
            _saveNodes = saveNodes;
            _loadGraphLogic = loadGraphLogic;
        }

        [Button]
        public void UpdateCompositions(string compositionID)
        {
            foreach (var group in trackObjectStorage.TrackObjectGroups.ToList())
            {
                bool updateSelf = compositionID == group.compositionID;

                GroupGameObjectSaveData data = composition.FindCompositionDataById(group.compositionID);
                List<TrackObjectPacket> trackObjectDatas = new List<TrackObjectPacket>();
                List<Track> tracks = new List<Track>();

                foreach (var child in data.children)
                {
                    if (child is GroupGameObjectSaveData groupChild)
                    {
                        if (updateSelf == false && compositionID != groupChild.compositionID)
                        {
                            continue;
                        }

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
                        if (!string.IsNullOrEmpty(keyframe.GetEntityData().Graph))
                        {
                            keyframe.GetEntityData().Logic = _saveNodes.LoadGraph(keyframe.GetEntityData().Graph,
                                TypeToDataType.Convert(keyframe.GetEntityData().GetType()),
                                keyframe.GetEntityData().initializedNodes, trackObjectDatas);
                        }
                    }
                }

                group.Update(data.duractionTime, trackObjectDatas, trackObjectRemover, _mainObjects,
                    keyframeTrackStorage, data.lastEditID, composition, compositionID, updateSelf, _loadGraphLogic);
            }
        }
    }
}