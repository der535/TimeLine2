using TimeLine.Test.Scripts;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine
{
    public partial struct RotationCubeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RotationSpeed>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (localTransform, rotationSpeed) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<RotationSpeed>>())
            {
                localTransform.ValueRW = localTransform.ValueRO.RotateZ(rotationSpeed.ValueRO.Value * SystemAPI.Time.DeltaTime);
            }
        }
    }
} 