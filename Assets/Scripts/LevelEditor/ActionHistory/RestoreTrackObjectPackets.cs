using System.Collections.Generic;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory
{
    public static class RestoreTrackObjectPackets
    {
        public static void RestoreLink(TrackObjectStorage trackObjectStorage, List<TrackObjectPacket> missingObjects, List<string> ids)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            for (int i = 0; i < ids.Count; i++)
            {
                // Unity-friendly проверка: если объект уничтожен, он будет "казаться" null.
                // Но так как TrackObjectPacket — это, скорее всего, обычный класс (C# object),
                // нам нужно проверить, не содержит ли он внутри себя уничтоженные Unity-компоненты.

                bool isInvalid = !entityManager.Exists(missingObjects[i].entity);


                if (isInvalid)
                {
                    // Debug.Log($"Restoring link for ID: {ids[i]}");
                    var restoredObject = trackObjectStorage.GetTrackObjectDataBySceneObjectID(ids[i]);

                    if (restoredObject != null)
                    {
                        missingObjects[i] = restoredObject;
                    }
                    else
                    {
                        Debug.LogError($"Failed to restore object with ID: {ids[i]}. It might not have been recreated yet.");
                    }
                }
            }
        }

        public static TrackObjectPacket RestoreLink(TrackObjectStorage trackObjectStorage, TrackObjectPacket missingObjects, string id)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Unity-friendly проверка: если объект уничтожен, он будет "казаться" null.
            // Но так как TrackObjectPacket — это, скорее всего, обычный класс (C# object),
            // нам нужно проверить, не содержит ли он внутри себя уничтоженные Unity-компоненты.

            bool isInvalid = !entityManager.Exists(missingObjects.entity);

            if (isInvalid)
            {
                // Debug.Log($"Restoring link for ID: {ids[i]}");
                var restoredObject = trackObjectStorage.GetTrackObjectDataBySceneObjectID(id);

                if (restoredObject != null)
                {
                    return restoredObject;
                }
                else
                {
                    Debug.LogError($"Failed to restore object with ID: {id}. It might not have been recreated yet.");
                }
            }

            return missingObjects;
        }
    }
}