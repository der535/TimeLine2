using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentInstaller;
using Unity.Entities;
using Unity.Mathematics;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentSaver
{
    public class SaveCircleCollider : IEntityComponentSave
    {
        CircleColliderInstaller _installer;

        public SaveCircleCollider(CircleColliderInstaller installer)
        {
            _installer = installer;
        }

        public (ComponentNames componentNames, Dictionary<string, object> save) Save(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (entityManager.HasComponent<CircleColliderData>(entity))
            {
                var collider = entityManager.GetComponentData<CircleColliderData>(entity);

                return (ComponentNames.CircleCollider, new Dictionary<string, object>()
                {
                    // Превращаем в массивы, которые JSON проглотит без проблем
                    {
                        "Radius",
                        collider.radius
                    },
                    {
                        "Center",
                        new[]
                        {
                            collider.center.x, collider.center.y
                        }
                    },
                    { "IsTrigger", collider.isTrigger },
                    { "IsDangerous", collider.isDangerous }
                });
            }
            else
            {
                return (ComponentNames.CircleCollider, null);
            }
        }


        public ComponentNames Check() => ComponentNames.CircleCollider;

        public void Load(Dictionary<string, object> data, Entity target)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // 1. Безопасное получение Radius
            // Convert.ToSingle корректно обработает и double, и int, и float
            float radius = System.Convert.ToSingle(data["Radius"]);

            // 2. Безопасное получение IsTrigger
            bool isTrigger = System.Convert.ToBoolean(data["IsTrigger"]);
            bool IsDangerous = System.Convert.ToBoolean(data["IsDangerous"]);

            // 3. Безопасное получение Center
            float[] centerData = GetFloatArray(data["Center"]);

            // Проверка на случай, если массив не загрузился
            if (centerData == null || centerData.Length < 2)
            {
                centerData = new float[] { 0, 0 };
            }

            CircleColliderData colliderData = new CircleColliderData()
            {
                radius = radius,
                center = new float3(centerData[0], centerData[1], 0),
                isTrigger = isTrigger,
                isDangerous = IsDangerous
            };

            entityManager.AddComponentData(target, colliderData);
            _installer.Install(target, colliderData);
        }

// Улучшенный вспомогательный метод
        float[] GetFloatArray(object obj)
        {
            if (obj == null) return null;

            // Если это JArray (стандарт для Newtonsoft при десериализации в object)
            if (obj is JArray jArray)
            {
                return jArray.ToObject<float[]>();
            }

            // Если это уже какой-то список или массив (например, после кэширования)
            if (obj is IEnumerable<object> list)
            {
                return list.Select(item => System.Convert.ToSingle(item)).ToArray();
            }

            if (obj is float[] direct) return direct;

            return null;
        }
    }
}