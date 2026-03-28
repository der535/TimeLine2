using System.Collections.Generic;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Misk
{
    public class FindField
    {
        private TrackObjectStorage _trackObjectStorage;

        [Inject]
        private void Construct(TrackObjectStorage trackObjectStorage)
        {
            _trackObjectStorage = trackObjectStorage;
        }

        public Entity? Find(MapParameterComponen MapParameter, List<TrackObjectPacket> objects = null)
        {
            var researchedObjects = objects ?? _trackObjectStorage.GetAllActiveTrackData();
            
            
            foreach (var VARIABLE in researchedObjects)
            {
                if (VARIABLE.sceneObjectID == MapParameter.SceneObjectID)
                {
                    return VARIABLE.entity;
                }
            }

            return null;
        }
    }
}