using System.Numerics;
using TimeLine.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.InspectorTab.InspectorView.Drawers;
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

            // УДАЛЯЕМ ЭТИ СТРОКИ: они — источник нормализации (0-360)
            // float3 eulerRadians = math.Euler(transform.Rotation);
            // float3 eulerDegrees = math.degrees(eulerRadians);
    
            // ВМЕСТО НИХ:
            // Мы берем 0, 0 для X и Y, потому что мы в 2D. 
            // Если объект наклонен по другим осям, эти данные должны быть в RotationData, а не доставаться из Кватерниона.
            float3 pureDegrees = new float3(0, 0, newRotation);
    
            // Используем FromEuler, который мы договорились использовать (из твоего GetDegree или напрямую)
            transform.Rotation = quaternion.EulerZXY(math.radians(pureDegrees));
    
            rotationData.RotateZ = newRotation;

            entityManager.SetComponentData(_trackObjectPacket.entity, transform);
            entityManager.SetComponentData(_trackObjectPacket.entity, rotationData);
    
            Debug.Log($"[Command] Applied Rotation: {newRotation} to Entity: {_trackObjectPacket.entity.Index}");
        }

        public void Undo()
        {
            _trackObjectPacket = RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, _trackObjectPacket, _savedID);
            ApplyPosition(_oldRotation);
            _transformComponentDrawer.UpdateValues.Invoke();
        }
    }
}