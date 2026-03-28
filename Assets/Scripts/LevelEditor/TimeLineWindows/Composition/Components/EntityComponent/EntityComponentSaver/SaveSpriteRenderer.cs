using System.Collections.Generic;
using Newtonsoft.Json;
using TimeLine.LevelEditor.SpriteLoader;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentSaver
{
    public class SaveSpriteRenderer : IEntityComponentSave
    {
        private readonly AddAnEntitySprite _addAnEntitySprite;
        private readonly BaseSpriteStorage _baseSpriteStorage;
        private readonly CustomSpriteStorage _customSpriteStorage;

        public SaveSpriteRenderer(AddAnEntitySprite addAnEntitySprite, BaseSpriteStorage baseSpriteStorage, CustomSpriteStorage customSpriteStorage)
        {
            _addAnEntitySprite = addAnEntitySprite;
            _baseSpriteStorage = baseSpriteStorage;
            _customSpriteStorage = customSpriteStorage;
        }

        public (ComponentNames componentNames, Dictionary<string, object> save) Save(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (entityManager.HasComponent<MaterialMeshInfo>(entity))
            {
                Material currentMat = null;
                RenderMeshArray rma = entityManager.GetSharedComponentManaged<RenderMeshArray>(entity);
                var meshInfo = entityManager.GetComponentData<MaterialMeshInfo>(entity);

                // Получаем текущий материал  
                currentMat = rma.GetMaterial(meshInfo);
                return (ComponentNames.SpriteRenderer, new Dictionary<string, object>()
                {
                    // Превращаем в массивы, которые JSON проглотит без проблем
                    { "Sprite name", currentMat.mainTexture.name },
                    { "Color", currentMat.color },
                });
            }

            return (ComponentNames.SpriteRenderer, null);
        }

        public ComponentNames Check() => ComponentNames.SpriteRenderer;

        public void Load(Dictionary<string, object> data, Entity target)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (data == null)
            {
                return;
            }

            
            // 1. Загрузка спрайта
            string spriteName = data["Sprite name"]?.ToString();
            Sprite sprite = _baseSpriteStorage.GetSprite(spriteName);
            if (sprite == null)
                sprite = _customSpriteStorage.GetSpriteFromID(spriteName);

            _addAnEntitySprite.SetupSpriteRender(target, sprite);

            // 2. Получаем материал
            RenderMeshArray rma = entityManager.GetSharedComponentManaged<RenderMeshArray>(target);
            var meshInfo = entityManager.GetComponentData<MaterialMeshInfo>(target);
            Material currentMat = rma.GetMaterial(meshInfo);

            // 3. БЕЗОПАСНАЯ загрузка цвета
            if (data.TryGetValue("Color", out object colorValue))
            {
                if (colorValue is Color directColor) 
                {
                    // Если мы копируем в памяти (Clipboard), это уже Color
                    currentMat.color = directColor;
                }
                else if (colorValue is Newtonsoft.Json.Linq.JObject jObject)
                {
                    // Если мы загружаем из файла, это JObject
                    currentMat.color = jObject.ToObject<Color>();
                }
                else if (colorValue is string jsonString && jsonString.StartsWith("{"))
                {
                    // Если это реально строка с JSON
                    currentMat.color = JsonConvert.DeserializeObject<Color>(jsonString);
                }
                else 
                {
                    Debug.LogWarning($"Unknown color format: {colorValue.GetType()}");
                }
            }
        }
    }
}