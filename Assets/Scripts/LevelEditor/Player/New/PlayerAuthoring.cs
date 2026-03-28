using Unity.Entities;
using Unity.Physics;
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
                // Получаем Entity, который Unity уже создает 
                // на основе PhysicsBody и других компонентов
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                // Просто добавляем наш тег к тому, что Unity уже создала
                AddComponent<PlayerTag>(entity);
            }
        }
    }
}