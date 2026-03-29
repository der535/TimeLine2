using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentInstaller
{
    public class SpriteRendererInstaller : IComponentInstaller
    {
        private readonly Material _baseMaterial;
        private readonly Mesh _quadMesh;

        // Передаем данные, необходимые для инициализации
        public SpriteRendererInstaller(Material baseMaterial, Mesh quadMesh)
        {
            _baseMaterial = baseMaterial;
            _quadMesh = quadMesh;
        }

        public ComponentNames GetComponentName()
        {
            return ComponentNames.SpriteRenderer;
        }

        public void Install(Entity entity)
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            Debug.Log(GetComponentName());

            entityManager.AddComponent<SpriteRendererTag>(entity);

            // 1. Создаем инстанс материала для текстуры спрайта
            Material instanceMat = new Material(_baseMaterial);

            // 2. Описываем массив мешей и материалов
            var renderMeshArray = new RenderMeshArray(new[] { instanceMat }, new[] { _quadMesh });
            
            var renderMeshDescription = new RenderMeshDescription
            {
                FilterSettings = RenderFilterSettings.Default,
                LightProbeUsage = LightProbeUsage.Off
            };

            // 3. Используем утилиту для добавления пакета компонентов
            RenderMeshUtility.AddComponents(
                entity, 
                entityManager, 
                renderMeshDescription, 
                renderMeshArray, 
                MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0)
            );
        }

        public void Remove(Entity entity)
        {
            Debug.Log("Remove");
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // 1. Очистка управляемых ресурсов (Материалы)
            if (entityManager.HasComponent<RenderMeshArray>(entity))
            {
                // Используем Managed метод для получения Shared компонента
                var rma = entityManager.GetSharedComponentManaged<RenderMeshArray>(entity);
                foreach (var mat in rma.MaterialReferences)
                {
                    if (mat != null) Object.Destroy(mat);
                }
            }

            // 2. Удаление пакета компонентов
            // Мы используем те типы, которые точно являются IComponentData
            var typesToRemove = new ComponentTypeSet(
                typeof(RenderMeshArray),
                typeof(MaterialMeshInfo),
                typeof(RenderFilterSettings),
                typeof(WorldRenderBounds),
                typeof(SpriteRendererTag)
            );

            entityManager.RemoveComponent(entity, typesToRemove);
        }
    }
}