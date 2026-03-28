using System;
using EventBus;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.EventBus.Events.TrackObject
{
    public struct AddComponentEvent : IEvent
    {
        public TrackObjectPacket TrackObjectPacket { get; }
        public ComponentNames ComponentType { get; }
        public Entity Entity { get; }

        public AddComponentEvent(TrackObjectPacket trackObjectPacket, ComponentNames componentType, Entity entity)
        {
            TrackObjectPacket = trackObjectPacket;
            ComponentType = componentType;
            Entity = entity;
        }
    }
}