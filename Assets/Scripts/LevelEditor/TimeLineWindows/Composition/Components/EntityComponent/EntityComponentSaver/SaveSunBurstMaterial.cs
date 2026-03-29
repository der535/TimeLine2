using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentInstaller;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentSaver
{
    public class SaveSunBurstMaterial : IEntityComponentSave
    {
        SunBurstMaterialInstaller sunBurstMaterialInstaller;

        public SaveSunBurstMaterial(SunBurstMaterialInstaller materialInstaller)
        {
            sunBurstMaterialInstaller = materialInstaller;
        }

        public (ComponentNames componentNames, Dictionary<string, object> save) Save(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (entityManager.HasComponent<SunBurstMaterialData>(entity))
            {
                var data = entityManager.GetComponentData<SunBurstMaterialData>(entity);
                Debug.Log(data.LineCount);
                Debug.Log(data.Offset);
                Debug.Log(data.TwistFactor);

                return (Check(), new Dictionary<string, object>()
                {
                    // Превращаем в массивы, которые JSON проглотит без проблем
                    {
                        "Color1",
                        new[] { data.Color1.r, data.Color1.g, data.Color1.b, data.Color1.a }
                    },
                    {
                        "Color2",
                        new[] { data.Color2.r, data.Color2.g, data.Color2.b, data.Color2.a }
                    },
                    { "LineCount", new float[] { data.LineCount } },
                    { "Offset", new float[] { data.Offset } },
                    { "TwistFactor", new float[] { data.TwistFactor } }
                });
            }
            else
            {
                return (Check(), null);
            }
        }


        public ComponentNames Check() => ComponentNames.SunBurstMaterial;

        public void Load(Dictionary<string, object> data, Entity target)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

           Debug.Log(JsonConvert.SerializeObject(data)); 

            float[] GetFloatArray(object obj)
            {
                if (obj == null) return null;
                if (obj is float[] directArray) return directArray;

                // 1. Пытаемся обработать как JObject (если есть $values)
                if (obj is JObject jObj)
                {
                    if (jObj.TryGetValue("$values", out JToken valuesToken))
                    {
                        return valuesToken.ToObject<float[]>();
                    }
                }

                // 2. Если это обычный JArray
                if (obj is JArray jArray) return jArray.ToObject<float[]>();

                // 3. Если это список double/object (fallback)
                if (obj is IEnumerable<object> list)
                {
                    var result = new List<float>();
                    foreach (var item in list) result.Add(System.Convert.ToSingle(item));
                    return result.ToArray();
                }

                return null;
            }

            // Извлекаем данные с проверкой на существование ключа
            float[] color1 = data.ContainsKey("Color1") ? GetFloatArray(data["Color1"]) : null;
            float[] color2 = data.ContainsKey("Color2") ? GetFloatArray(data["Color2"]) : null;
            float[] lineCount = data.ContainsKey("LineCount") ? GetFloatArray(data["LineCount"]) : null;
            float[] offset = data.ContainsKey("Offset") ? GetFloatArray(data["Offset"]) : null;
            float[] twist = data.ContainsKey("TwistFactor") ? GetFloatArray(data["TwistFactor"]) : null;

            // Проверка на null перед созданием структуры, чтобы избежать NullReferenceException
            if (color1 == null || color2 == null || lineCount == null || offset == null || twist == null)
            {
                Debug.LogError("SaveSunBurstMaterial: Не удалось загрузить данные, один из массивов null!");
                return;
            }

            SunBurstMaterialData sunBurstMaterialData = new SunBurstMaterialData()
            {
                Color1 = new Color(color1[0], color1[1], color1[2], color1[3]),
                Color2 = new Color(color2[0], color2[1], color2[2], color2[3]),
                LineCount = (int)lineCount[0],
                Offset = offset[0],
                TwistFactor = twist[0],
            };

            entityManager.AddComponentData(target, sunBurstMaterialData);

            // Обязательно проверяем инсталлер
            if (sunBurstMaterialInstaller != null)
            {
                sunBurstMaterialInstaller.Install(target, sunBurstMaterialData);
            }
        }
    }
}