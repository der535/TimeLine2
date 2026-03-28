using TimeLine.LevelEditor.ECS.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

public partial struct DeactivationCleanupSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Ищем только тех, кто в процессе деактивации
        foreach (var (tag, entity) in SystemAPI.Query<DeactivatingRequestTag>().WithEntityAccess())
        {
            // Убираем временный тег, чтобы не повторять действие
            ecb.RemoveComponent<DeactivatingRequestTag>(entity);
            
            // Если нужно выключить графику вручную (если она не смотрит на EntityActiveTag)
            if (SystemAPI.HasComponent<MaterialMeshInfo>(entity))
            {
                SystemAPI.SetComponentEnabled<MaterialMeshInfo>(entity, false);
            }
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public partial struct ActivationCleanupSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Ищем только тех, кто в процессе деактивации
        foreach (var (tag, entity) in SystemAPI.Query<ActivatingRequestTag>().WithEntityAccess())
        {
            // Убираем временный тег, чтобы не повторять действие
            ecb.RemoveComponent<ActivatingRequestTag>(entity);
            
            // Если нужно выключить графику вручную (если она не смотрит на EntityActiveTag)
            if (SystemAPI.HasComponent<MaterialMeshInfo>(entity))
            {
                SystemAPI.SetComponentEnabled<MaterialMeshInfo>(entity, true);
            }
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}