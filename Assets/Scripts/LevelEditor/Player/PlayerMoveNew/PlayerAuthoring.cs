using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine
{
    public struct PlayerTag : IComponentData {}
    
    public class PlayerAuthoring : MonoBehaviour
    {
        // Теперь Unity увидит этот Baker и применит его 
        // ДОПОЛНИТЕЛЬНО к тому, что она сама делает с компонентами типа PhysicsBody
        private class PlayerBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                // 1. Создаем Entity
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // 2. В Baker мы берем данные напрямую из authoring (GameObject)
                // Unity сама сконвертирует Transform объекта в LocalTransform сущности.
                // Но если вам нужны значения прямо сейчас для расчетов:
                float scale = authoring.transform.localScale.x; 

                // 3. Добавляем компоненты
                AddComponent<PostTransformMatrix>(entity);
                AddComponent<PlayerTag>(entity);
        
                // 4. Устанавливаем значения
                SetComponent(entity, new PostTransformMatrix
                {
                    // Используем масштаб из authoring
                    Value = float4x4.Scale(new float3(scale, scale, scale))
                });
            }
        }
    }
}