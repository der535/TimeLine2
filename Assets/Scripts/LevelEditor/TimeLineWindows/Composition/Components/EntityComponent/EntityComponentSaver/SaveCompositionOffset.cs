using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentSaver
{
    public class SaveCompositionOffset : IEntityComponentSave
    {
        public (ComponentNames componentNames, Dictionary<string, object> save) Save(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (entityManager.HasComponent<CompositionPositionOffsetData>(entity))
            {
                var offset = entityManager.GetComponentData<CompositionPositionOffsetData>(entity);

                return (ComponentNames.CompositionOffset, new Dictionary<string, object>()
                {
                    // Превращаем в массивы, которые JSON проглотит без проблем
                    {
                        "Offset",
                        new[] { offset.Offset.x, offset.Offset.y }
                    },
                });

            }
            else
            {
                return (ComponentNames.CompositionOffset, null);
            }
        }


        public ComponentNames Check() => ComponentNames.CompositionOffset;

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

            float[] offset = GetFloatArray(data["Offset"]);
            
            if (entityManager.HasComponent<CompositionPositionOffsetData>(target))
                entityManager.SetComponentData(target, new CompositionPositionOffsetData
                {
                    Offset = new float2(offset[0], offset[1])
                });
            else
            {
                entityManager.AddComponentData(target, new CompositionPositionOffsetData
                {
                    Offset = new float2(offset[0], offset[1])
                });
            }
            
        }
        
     
        
    }
    
}