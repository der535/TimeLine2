using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentInstaller;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentSaver
{
    public class SavePolygonCollider : IEntityComponentSave
    {
        private PolygoneColliderInstaller _installer;

        public SavePolygonCollider(PolygoneColliderInstaller installer)
        {
            _installer = installer;
        }

        public (ComponentNames componentNames, Dictionary<string, object> save) Save(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (!entityManager.HasComponent<PolygonColliderData>(entity))
                return (ComponentNames.PolygonCollider, null);

            var collider = entityManager.GetComponentData<PolygonColliderData>(entity);

            // 1. Безопасная проверка Blob-ссылки
            if (!collider.PointsReference.IsCreated)
            {
                Debug.LogWarning("BlobAssetReference не создан!");
                return (ComponentNames.PolygonCollider, null);
            }

            // 2. Конвертируем float2 в обычный массив float или Vector2
            // Это гарантирует, что сериализатор не уйдет в бесконечный цикл
            var points = collider.PointsReference.Value.Points.ToArray();
            var serializablePoints = new List<float[]>();

            for (int i = 0; i < points.Length; i++)
            {
                serializablePoints.Add(new float[] { points[i].x, points[i].y });
            }

            return (ComponentNames.PolygonCollider, new Dictionary<string, object>()
            {
                { "Points", serializablePoints }, // Массив простых массивов
                { "IsTrigger", collider.IsTrigger },
                { "IsDangerous", collider.IsDangerous }
            });
        }


        public ComponentNames Check()
        {
            return ComponentNames.PolygonCollider;
        }

        public void Load(Dictionary<string, object> data, Entity target)
        {
            // 1. Безопасное получение Radius
            // Convert.ToSingle корректно обработает и double, и int, и float

            List<float[]> points = (List<float[]>)data["Points"];

            // 2. Безопасное получение IsTrigger
            bool isTrigger = System.Convert.ToBoolean(data["IsTrigger"]);
            bool IsDangerous = System.Convert.ToBoolean(data["IsDangerous"]);

            PolygonColliderData colliderData = new PolygonColliderData()
            {
                PointsReference = CreateBlobFromPoints(points),
                IsTrigger = isTrigger,
                IsDangerous = IsDangerous
            };

            _installer.Install(target, colliderData);
        }

        public BlobAssetReference<PolygonPointsBlob> CreateBlobFromPoints(List<float[]> points)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<PolygonPointsBlob>();

            var arrayBuilder = builder.Allocate(ref root.Points, points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                arrayBuilder[i] = new float2(points[i][0], points[i][1]);
            }

            var blobReference = builder.CreateBlobAssetReference<PolygonPointsBlob>(Allocator.Persistent);
            builder.Dispose();

            return blobReference;
        }
    }
}