using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimeLine.LevelEditor.ECS;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentSaver
{
    public class SaveTransform : IEntityComponentSave
    {
        public (ComponentNames componentNames, Dictionary<string, object> save) Save(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var localTransform = entityManager.GetComponentData<LocalTransform>(entity);
            var rotation = GetDegree.FromQuaternion(localTransform.Rotation);
            var rotationData = entityManager.GetComponentData<RotationData>(entity);
            var ptm = entityManager.GetComponentData<PostTransformMatrix>(entity);
            var positionData = entityManager.GetComponentData<PositionData>(entity);

            return (ComponentNames.Transform, new Dictionary<string, object>()
            {
                // Превращаем в массивы, которые JSON проглотит без проблем
                {
                    "Position",
                    new[] { positionData.Position.x, positionData.Position.y, localTransform.Position.z }
                }, {
                    "Rotation",
                    new[]
                    {
                        rotation.x, rotation.y,
                        rotationData.RotateZ
                    }
                },
                { "ScaleMatrix", SerializeMatrix(ptm.Value) }
            });
        }

// Вспомогательный метод для матрицы
        private float[] SerializeMatrix(float4x4 m)
        {
            return new[]
            {
                m.c0.x, m.c0.y, m.c0.z, m.c0.w,
                m.c1.x, m.c1.y, m.c1.z, m.c1.w,
                m.c2.x, m.c2.y, m.c2.z, m.c2.w,
                m.c3.x, m.c3.y, m.c3.z, m.c3.w
            };
        }

        public ComponentNames Check() => ComponentNames.Transform;

        public void Load(Dictionary<string, object> data, Entity target)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Вспомогательная функция для безопасного получения float[]
            float[] GetFloatArray(object obj)
            {
                if (obj is float[] directArray) return directArray; // Если это уже массив
                if (obj is JArray jArray) return jArray.ToObject<float[]>(); // Если это JArray из JSON

                // На случай, если Newtonsoft десериализовал это как список double (бывает по умолчанию)
                if (obj is IEnumerable<object> list)
                {
                    var result = new List<float>();
                    foreach (var item in list) result.Add(System.Convert.ToSingle(item));
                    return result.ToArray();
                }

                return null;
            }

            float[] posData = GetFloatArray(data["Position"]);
            float[] rotData = GetFloatArray(data["Rotation"]);
            float[] scaleData = GetFloatArray(data["ScaleMatrix"]);

            if (posData == null || rotData == null || scaleData == null)
            {
                Debug.LogError("Failed to parse Transform data. Object type: " + data["Position"].GetType());
                return;
            }

            
            // 2. Собираем LocalTransform
            LocalTransform lt = new LocalTransform
            {
                Position = new float3(posData[0], posData[1], posData[2]),
                Rotation = GetDegree.FromEuler(new Vector3(rotData[0], rotData[1], rotData[2])),
                Scale = 1.0f
            };

            // 3. Собираем матрицу
            float4x4 matrix = new float4x4(
                new float4(scaleData[0], scaleData[1], scaleData[2], scaleData[3]),
                new float4(scaleData[4], scaleData[5], scaleData[6], scaleData[7]),
                new float4(scaleData[8], scaleData[9], scaleData[10], scaleData[11]),
                new float4(scaleData[12], scaleData[13], scaleData[14], scaleData[15])
            );

            PostTransformMatrix ptm = new PostTransformMatrix { Value = matrix };

            // 4. Запись
            if (entityManager.HasComponent<LocalTransform>(target))
                entityManager.SetComponentData(target, lt);

            if (entityManager.HasComponent<PositionData>(target))
            {
                PositionData positionData = new PositionData
                {
                    Position = new float2(posData[0], posData[1])
                };
                entityManager.SetComponentData(target, positionData);
            }

            if (entityManager.HasComponent<PostTransformMatrix>(target))
                entityManager.SetComponentData(target, ptm);
        }
    }
}