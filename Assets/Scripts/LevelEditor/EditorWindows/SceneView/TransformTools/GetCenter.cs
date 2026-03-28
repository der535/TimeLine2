using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine
{
    public static class GetCenter
    {
        public static Vector2 GetSelectionCenter(List<Transform> selection)
        {
            Vector2 center = Vector2.zero;
            foreach (Transform pos in selection)
                center += (Vector2)pos.position;
            center /= selection.Count;
            return center;
        }
        
        public static Vector2 GetSelectionCenter(List<LocalTransform> selection)
        {
            Vector2 center = Vector2.zero;
            foreach (var pos in selection)
                center += new Vector2(pos.Position.x, pos.Position.y);
            center /= selection.Count;
            return center;
        }

        public static Vector2 GetSelectionCenter(List<Entity> selection)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            Vector2 center = Vector2.zero;
            foreach (var entity in selection)
            {
                float3 position = entityManager.GetComponentData<LocalTransform>(entity).Position;
                center += new Vector2(position.x, position.y);
            }
            center /= selection.Count;
            return center;
        }
        
    }
}