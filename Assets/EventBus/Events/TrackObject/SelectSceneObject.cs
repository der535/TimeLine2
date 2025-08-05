using EventBus;
using UnityEngine;

namespace TimeLine.EventBus.Events.TrackObject
{
    internal struct SelectSceneObject : IEvent
    {
        public GameObject GameObject { get; }
        public SelectSceneObject(GameObject gameObject)
        {
            GameObject = gameObject;
        }
    }
}