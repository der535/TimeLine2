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

        public InspectableParameter Find(MapParameterComponen MapParameter, List<TrackObjectData> objects = null)
        {
            Debug.Log(MapParameter.SceneObjectID);
            Debug.Log(MapParameter.ComponentID);
            Debug.Log(MapParameter.ParameterID);
            List<TrackObjectData> researchedObjects = new List<TrackObjectData>();
            
            if (objects == null) researchedObjects = _trackObjectStorage.GetAllActiveTrackData();
            else researchedObjects = objects;
            
            Debug.Log(researchedObjects.Count);
            
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
                                    
                                    Debug.Log(param);

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