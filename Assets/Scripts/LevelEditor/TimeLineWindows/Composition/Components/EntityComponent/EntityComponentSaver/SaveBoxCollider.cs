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
    public class SaveBoxCollider : IEntityComponentSave
    {
        BoxColliderInstaller _boxColliderInstaller;
        public SaveBoxCollider(BoxColliderInstaller boxColliderInstaller)
        {
            _boxColliderInstaller = boxColliderInstaller;
        }

        public (ComponentNames componentNames, Dictionary<string, object> save) Save(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (entityManager.HasComponent<BoxColliderData>(entity))
            {
                var boxCollider = entityManager.GetComponentData<BoxColliderData>(entity);

                return (ComponentNames.BoxCollider, new Dictionary<string, object>()
                {
                    // Превращаем в массивы, которые JSON проглотит без проблем
                    {
                        "Scale",
                        new float[] { boxCollider.boxSize.x, boxCollider.boxSize.y }
                    },
                    {
                        "Center",
                        new float[]
                        {
                            boxCollider.boxCenter.x, boxCollider.boxCenter.y
                        }
                    },
                    { "IsTrigger", boxCollider.isTrigger },
                    { "IsDangerous", boxCollider.isDangerous }
                });
            }
            else
            {
                return (ComponentNames.BoxCollider, null);
            }

        }


        public ComponentNames Check() => ComponentNames.BoxCollider;

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

            float[] scaleData = GetFloatArray(data["Scale"]);
            float[] canterData = GetFloatArray(data["Center"]);
            bool isTrigger = (bool)data["IsTrigger"];
            bool IsDangerous = (bool)data["IsDangerous"];

            BoxColliderData boxColliderData = new BoxColliderData()
            {
                boxSize = new float3(scaleData[0], scaleData[1], 100),
                boxCenter = new float3(canterData[0], canterData[1], 0),
                isTrigger = isTrigger,
                isDangerous = IsDangerous,
            };

            entityManager.AddComponentData(target, boxColliderData);
            
            _boxColliderInstaller.Install(target, boxColliderData);
        }
    }
}