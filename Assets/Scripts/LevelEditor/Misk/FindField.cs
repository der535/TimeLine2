using System.Collections.Generic;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
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

        public InspectableParameter Find(MapParameterComponen MapParameter, List<TrackObjectPacket> objects = null)
        {
            List<TrackObjectPacket> researchedObjects = new List<TrackObjectPacket>();
            
            if (objects == null) researchedObjects = _trackObjectStorage.GetAllActiveTrackData();
            else researchedObjects = objects;
            
            
            foreach (var VARIABLE in researchedObjects)
            {
                if (VARIABLE.sceneObjectID == MapParameter.SceneObjectID)
                {
                    var components = VARIABLE.sceneObject.GetComponents<BaseParameterComponent>();
                    foreach (var component in components)
                    {
                        if (component.GetID() == MapParameter.ComponentID)
                        {
                            var parameters = component.GetParameters();
                            foreach (var param in parameters)
                            {
                                if (param.Id == MapParameter.ParameterID)
                                {
                                    return param;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}