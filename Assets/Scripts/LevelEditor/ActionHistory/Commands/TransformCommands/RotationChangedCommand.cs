using System.Numerics;
using TimeLine.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class RotationChangedCommand : ICommand
    {
        private TrackObjectPacket _trackObjectPacket;
        private readonly TransformComponentDrawer _transformComponentDrawer;
        private readonly TrackObjectStorage _trackObjectStorage;
        private float _newRotation, _oldRotation;
        private readonly string _description;
        private string _savedID;

        public RotationChangedCommand(float newRotation, TransformComponentDrawer transformComponentDrawer, TrackObjectPacket trackObjectPacket, TrackObjectStorage trackObjectStorage, string description)
        {
            _transformComponentDrawer = transformComponentDrawer;
            _description = description;
            _trackObjectPacket = trackObjectPacket;
            _newRotation = newRotation;
            _trackObjectStorage = trackObjectStorage;
        }

        public string Description() => _description;

        public void Execute()
        {
            _oldRotation = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<RotationData>(_trackObjectPacket.entity).RotateZ;
            ApplyPosition(_newRotation);
        }

        private void ApplyPosition(float newRotation)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            LocalTransform transform = entityManager.GetComponentData<LocalTransform>(_trackObjectPacket.entity);
            RotationData rotationData = entityManager.GetComponentData<RotationData>(_trackObjectPacket.entity);

            float3 eulerRadians = math.Euler(transform.Rotation);
            float3 eulerDegrees = math.degrees(eulerRadians);
            
            eulerDegrees.z = newRotation;
            transform.Rotation = quaternion.Euler(math.radians(eulerDegrees));
            rotationData.RotateZ = newRotation;
            entityManager.SetComponentData(_trackObjectPacket.entity, transform);
            entityManager.SetComponentData(_trackObjectPacket.entity, rotationData);
        }

        public void Undo()
        {
            _trackObjectPacket = RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, _trackObjectPacket, _savedID);
            ApplyPosition(_oldRotation);
            _transformComponentDrawer.UpdateValues.Invoke();
        }
    }
}