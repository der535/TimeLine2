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
        // Unity увидит этот Baker и применит его
        private class PlayerBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                // 1. Создаем Entity с флагами динамического трансформа.
                // Unity САМА добавит компонент LocalTransform и запишет туда масштаб из объекта!
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // 2. Добавляем только наш тэг. 
                // Больше никаких дублирующихся PostTransformMatrix!
                AddComponent<PlayerTag>(entity);
                
                // 💡 Полезная заметка:
                // Если вам ВДРУГ позарез нужно жестко переписать масштаб кодом именно в бейкере, 
                // правильнее делать это через изменение LocalTransform, а не PostTransformMatrix:
                /*
                var transform = LocalTransform.FromPositionRotationScale(
                    authoring.transform.position,
                    authoring.transform.rotation,
                    authoring.transform.localScale.x
                );
                AddComponent(entity, transform);
                */
            }
        }
    }
}