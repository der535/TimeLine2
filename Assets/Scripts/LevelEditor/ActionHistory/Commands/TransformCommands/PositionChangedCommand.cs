using System.Numerics;
using TimeLine.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class PositionChangedCommand : ICommand
    {
        private TrackObjectPacket _trackObjectPacket;
        private TransformComponentDrawer _transformComponentDrawer;
        private TrackObjectStorage _trackObjectStorage;
        private float2 _newPosition, _oldPosition;
        private readonly string _description;
        private string _savedID;

        public PositionChangedCommand(string description, float2 newPosition, TransformComponentDrawer transformComponentDrawer, TrackObjectPacket trackObjectPacket, TrackObjectStorage trackObjectStorage)
        {
            _transformComponentDrawer = transformComponentDrawer;
            _description = description;
            _trackObjectPacket = trackObjectPacket;
            _newPosition = newPosition;
            _trackObjectStorage = trackObjectStorage;
        }

        public string Description() => _description;

        public void Execute()
        {
            _oldPosition = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<PositionData>(_trackObjectPacket.entity).Position;
            ApplyPosition(_newPosition);
        }

        private void ApplyPosition(float2 newPosition)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            LocalTransform transform = entityManager.GetComponentData<LocalTransform>(_trackObjectPacket.entity);
            PositionData positionData = entityManager.GetComponentData<PositionData>(_trackObjectPacket.entity);
            ObjectPositionOffsetData objectPositionOffsetData = entityManager.GetComponentData<ObjectPositionOffsetData>(_trackObjectPacket.entity);

            positionData.Position = newPosition;
            transform.Position.xy = newPosition + objectPositionOffsetData.Offset.xy;

            Debug.Log(positionData.Position);
            
            entityManager.SetComponentData(_trackObjectPacket.entity, transform);
            entityManager.SetComponentData(_trackObjectPacket.entity, positionData);
            entityManager.SetComponentData(_trackObjectPacket.entity, objectPositionOffsetData);
        }

        public void Undo()
        {
            _trackObjectPacket = RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, _trackObjectPacket, _savedID);
            ApplyPosition(_oldPosition);
            _transformComponentDrawer.UpdateValues.Invoke();
        }
    }
}