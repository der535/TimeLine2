using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using TimeLine.Installers;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class CompositionUpdater : MonoBehaviour
    {
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private TrackObjectSpawner trackObjectSpawner;
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
        public void UpdateCompositions()
        {
            foreach (var group in trackObjectStorage.TrackObjectGroups.ToList())
            {
                GroupGameObjectSaveData data = composition.FindCompositionDataById(group.compositionID);
                List<TrackObjectData> trackObjectDatas = new List<TrackObjectData>();

                foreach (var child in data.children)
                {
                    if (child is GroupGameObjectSaveData groupChild)
                    {
                        GroupGameObjectSaveData groupChildData =
                            composition.FindCompositionDataById(groupChild.compositionID);
                        if (groupChildData != null)
                        {
                            var (trackData, _, _) = trackObjectSpawner.LoadGroup(groupChild, groupChild.compositionID,
                                groupChildData, false);
                            trackObjectDatas.Add(trackData);
                        }
                    }
                    else
                    {
                        var (trackData, _, _) = trackObjectSpawner.LoadTrackObject(child, false);
                        trackObjectDatas.Add(trackData);
                    }
                }

                group.Update(data.duractionTime, trackObjectDatas, trackObjectRemover, _mainObjects,
                    keyframeTrackStorage);
            }
        }
    }
}