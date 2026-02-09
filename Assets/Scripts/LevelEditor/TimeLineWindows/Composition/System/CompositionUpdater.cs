using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using TimeLine.Installers;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class CompositionUpdater : MonoBehaviour
    {
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [FormerlySerializedAs("objectSpawner")] [FormerlySerializedAs("trackObjectSpawner")] [SerializeField] private FacadeObjectSpawner facadeObjectSpawner;
        [SerializeField] private SaveComposition composition;
        [SerializeField] private TrackObjectRemover trackObjectRemover;
        [SerializeField] private KeyframeTrackStorage keyframeTrackStorage;

        private MainObjects _mainObjects;

        [Inject]
        private void Construct(MainObjects mainObjects)
        {
            _mainObjects = mainObjects;
        }

        [Button]
        public void UpdateCompositions(string compositionID)
        {
            foreach (var group in trackObjectStorage.TrackObjectGroups.ToList())
            {
                bool updateSelf = compositionID == group.compositionID;
                
                GroupGameObjectSaveData data = composition.FindCompositionDataById(group.compositionID);
                List<TrackObjectData> trackObjectDatas = new List<TrackObjectData>();
                
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
                            var (trackData, _, _) = facadeObjectSpawner.LoadComposition(groupChild, groupChild.compositionID,
                              false,  groupChildData, false, lastEditID:data.lastEditID);
                            trackObjectDatas.Add(trackData);
                        }
                    }
                    else
                    {
                        if (updateSelf)
                        {
                            var (trackData, _, _) = facadeObjectSpawner.LoadObject(child, false);
                            trackObjectDatas.Add(trackData);
                        }

                    }
                }
                
                group.Update(data.duractionTime, trackObjectDatas, trackObjectRemover, _mainObjects,
                    keyframeTrackStorage, data.lastEditID, composition,compositionID, updateSelf);
            }
        }
    }
}