using System.Numerics;
using TimeLine.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Scale;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.LevelEditor.ActionHistory.Commands
{
    public class ScaleChangedCommand : ICommand
    {
        private TrackObjectPacket _trackObjectPacket;
        private readonly TransformComponentDrawer _transformComponentDrawer;
        private readonly TrackObjectStorage _trackObjectStorage;
        private float2 _newScale, _oldScale;
        private readonly string _description;
        private string _savedID;

        public ScaleChangedCommand(float2 newScale, TransformComponentDrawer transformComponentDrawer, TrackObjectPacket trackObjectPacket, TrackObjectStorage trackObjectStorage, string description)
        {
            _transformComponentDrawer = transformComponentDrawer;
            _description = description;
            _trackObjectPacket = trackObjectPacket;
            _newScale = newScale;
            _trackObjectStorage = trackObjectStorage;
        }

        public string Description() => _description;

        public void Execute()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _oldScale = entityManager.GetComponentData<PostTransformMatrix>(_trackObjectPacket.entity).Value.Scale().xy;
            ApplyScale(_newScale);
        }

        private void ApplyScale(float2 newScale)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var postMatrix = entityManager.GetComponentData<PostTransformMatrix>(_trackObjectPacket.entity);
            float3 nonUniformScale =
                new float3(postMatrix.Value.c0.x, postMatrix.Value.c1.y, postMatrix.Value.c2.z);
            nonUniformScale.xy = newScale;
            postMatrix.Value = float4x4.Scale(nonUniformScale);
            entityManager.SetComponentData(_trackObjectPacket.entity, postMatrix);

        }

        public void Undo()
        {
            _trackObjectPacket = RestoreTrackObjectPackets.RestoreLink(_trackObjectStorage, _trackObjectPacket, _savedID);
            ApplyScale(_oldScale);
            _transformComponentDrawer.UpdateValues.Invoke();
        }
    }
}