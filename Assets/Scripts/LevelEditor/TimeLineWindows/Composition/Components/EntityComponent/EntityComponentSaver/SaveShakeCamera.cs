using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TimeLine.LevelEditor.SpriteLoader;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentSaver
{
    public class SaveShakeCamera : IEntityComponentSave
    {
        public SaveShakeCamera(AddAnEntitySprite addAnEntitySprite, BaseSpriteStorage baseSpriteStorage,
            CustomSpriteStorage customSpriteStorage)
        {
        }

        public (ComponentNames componentNames, Dictionary<string, object> save) Save(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (entityManager.HasComponent<ShakeCameraData>(entity))
            {
                ShakeCameraData shakeCameraData = entityManager.GetComponentData<ShakeCameraData>(entity);

                return (Check(), new Dictionary<string, object>()
                {
                    // Превращаем в массивы, которые JSON проглотит без проблем
                    { "StrengthX", new[] { shakeCameraData.StrengthX }},
                    { "StrengthY", new[] { shakeCameraData.StrengthY }},
                    { "Duration",new[] { shakeCameraData.Duration }},
                    { "Vibrato",new[] { shakeCameraData.Vibrato }},
                    { "Randomness",new[] { shakeCameraData.Randomness }},
                });
            }

            return (Check(), null);
        }

        public ComponentNames Check() => ComponentNames.ShakeCamera;

        public void Load(Dictionary<string, object> data, Entity target)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (data == null)
            {
                return;
            }

            float[] GetFloatArray(string key)
            {
                if (data.TryGetValue(key, out object value) == false)
                {
                    return  new float[] { 0 };
                }
                
                
                if (value is float[] directArray) return directArray; // Если это уже массив
                if (value is JArray jArray) return jArray.ToObject<float[]>(); // Если это JArray из JSON

                // На случай, если Newtonsoft десериализовал это как список double (бывает по умолчанию)
                if (value is IEnumerable<object> list)
                {
                    var result = new List<float>();
                    foreach (var item in list) result.Add(System.Convert.ToSingle(item));
                    return result.ToArray();
                }

                return new float[] { 0 };
            }


            
            float StrengthX = GetFloatArray("StrengthX")[0];
            float StrengthY = GetFloatArray("StrengthY")[0];
            float Duration = GetFloatArray("Duration")[0];
            int Vibrato = (int)GetFloatArray("Vibrato")[0];
            float Randomness = GetFloatArray("Randomness")[0];

            entityManager.AddComponent<ShakeCameraData>(target);
            ShakeCameraData shakeCameraData = new ShakeCameraData()
            {
                StrengthX = StrengthX,
                StrengthY = StrengthY,
                Duration = Duration,
                Vibrato = Vibrato,
                Randomness = Randomness
            };
            entityManager.AddComponentData(target, shakeCameraData);
        }
    }
}