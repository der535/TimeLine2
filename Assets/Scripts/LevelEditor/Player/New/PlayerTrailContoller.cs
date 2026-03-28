using System;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine
{
    public class PlayerTrailContoller : MonoBehaviour
    {
        private Entity player;
        private void Start()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = entityManager.CreateEntityQuery(typeof(PlayerTag));
            player = query.GetSingletonEntity();
        }

        void Update()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            transform.position = entityManager.GetComponentData<LocalToWorld>(player).Position;
        }
    }
}
