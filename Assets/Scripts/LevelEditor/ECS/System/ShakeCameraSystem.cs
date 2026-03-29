using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.LevelEditor.ECS.System
{
    public partial struct ShakeCameraSystem : ISystem
    {    
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Ищем только тех, кто в процессе деактивации
            foreach (var (shakeCameraData, entity) in SystemAPI.Query<RefRW<ShakeCameraData>>().WithEntityAccess())
            {
                double time = ECSServiceLocator.Instance.TrackObjectStorage.GetTrackObjectData(entity).components.Data.StartTimeInTicks;
                double currentTime = ECSServiceLocator.Instance.M_PlaybackState.SmoothTimeInTicks;
            
                if (currentTime >= time)
                {
                    if (shakeCameraData.ValueRO.IsInitialized == false)
                    {
                        shakeCameraData.ValueRW.IsInitialized = true;
                        ECSServiceLocator.Instance.ShakeCameraController.Shake(new Vector2(shakeCameraData.ValueRO.StrengthX, shakeCameraData.ValueRO.StrengthY),
                            shakeCameraData.ValueRO.Duration, shakeCameraData.ValueRO.Vibrato, shakeCameraData.ValueRO.Randomness);
                    }
                }
                else
                {
                    if (shakeCameraData.ValueRO.IsInitialized) shakeCameraData.ValueRW.IsInitialized = false;
                }
            
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
