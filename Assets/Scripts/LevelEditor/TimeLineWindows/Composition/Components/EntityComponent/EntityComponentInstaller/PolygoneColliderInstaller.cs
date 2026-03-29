using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using UnityEngine;
using Collider = Unity.Physics.Collider;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentInstaller
{
    public class PolygoneColliderInstaller : IComponentInstaller
    {
        public ComponentNames GetComponentName()
        {
            return ComponentNames.PolygonCollider;
        }

        public void Install(Entity entity)
        {
            var points = new NativeArray<float2>(3, Allocator.Temp);
            points[0] = new float2(1, 0);
            points[1] = new float2(-1, 0);
            points[2] = new float2(0, 1);

            // 2. Создаем физический коллайдер
            var blobRef = CreateBlobFromPoints(points);
            Install(entity, new PolygonColliderData
            {
                PointsReference = blobRef,
                IsTrigger = false
            });
            points.Dispose();
        }

        public void Install(Entity entity, PolygonColliderData data)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            BlobAssetReference<Collider> collider =
                TimeLineConverter.InstallConvexTriangle(data.PointsReference.Value.Points.ToArray());

            // 1. Создаем точки
            entityManager.AddComponentData(entity, new PhysicsCollider { Value = collider });

            // 3. Создаем Blob с точками и вешаем компонент-тег
            entityManager.AddComponentData(entity, data);


            Debug.Log("Polygon Collider and Tag installed successfully");

            entityManager.AddSharedComponentManaged(entity, new PhysicsWorldIndex
            {
                Value = 0
            });
        }

        public BlobAssetReference<PolygonPointsBlob> CreateBlobFromPoints(NativeArray<float2> points)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<PolygonPointsBlob>(); // Корень теперь PolygonPointsBlob

            var arrayBuilder = builder.Allocate(ref root.Points, points.Length);
            for (int i = 0; i < points.Length; i++)
            {
                arrayBuilder[i] = points[i];
            }

            // Используем Persistent, так как Blob должен жить долго
            var blobReference = builder.CreateBlobAssetReference<PolygonPointsBlob>(Allocator.Persistent);
            builder.Dispose();
            return blobReference;
        }

        public void Remove(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (entityManager.HasComponent<PolygonColliderData>(entity))
            {
                // 1. Get the component to access the reference
                var tag = entityManager.GetComponentData<PolygonColliderData>(entity);

                // 2. Dispose the blob
                if (tag.PointsReference.IsCreated)
                {
                    tag.PointsReference.Dispose();
                }

                // 3. Now remove the components
                entityManager.RemoveComponent<PolygonColliderData>(entity);
            }

            // Handle PhysicsCollider separately as it's a separate BlobAsset
            if (entityManager.HasComponent<PhysicsCollider>(entity))
            {
                var physicsCollider = entityManager.GetComponentData<PhysicsCollider>(entity);
                // If you created this manually as Persistent, dispose it here too
                if (physicsCollider.Value.IsCreated)
                {
                    physicsCollider.Value.Dispose();
                }

                entityManager.RemoveComponent<PhysicsCollider>(entity);
            }
        }
    }
}