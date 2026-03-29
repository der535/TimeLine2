using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentInstaller
{
    public class SunBurstMaterialInstaller : IComponentInstaller
    {
        private Material _sunBurstMaterialTemplate;
        private Mesh _quadMesh;
        private Sprite _quadSprite;

        public SunBurstMaterialInstaller(Material sunBurstMaterialTemplate, Mesh quadMesh, Sprite quadSprite)
        {
            _sunBurstMaterialTemplate = sunBurstMaterialTemplate;
            _quadMesh = quadMesh;
            _quadSprite = quadSprite;
        }

        public ComponentNames GetComponentName()
        {
            return ComponentNames.SunBurstMaterial;
        }

        public void Install(Entity entity)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var data = new SunBurstMaterialData()
            {
                Color1 = Color.white,
                Color2 = Color.black,
                LineCount = 5,
                TwistFactor = 3
            };
            entityManager.AddComponent<SunBurstMaterialData>(entity);
            entityManager.SetComponentData(entity, data);
            
            SetupMaterial(entity, data);
        }
        
        public void Install(Entity entity, SunBurstMaterialData sunBurstMaterialData)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            entityManager.AddComponent<SunBurstMaterialData>(entity);
            entityManager.SetComponentData(entity, sunBurstMaterialData);
            
            SetupMaterial(entity, sunBurstMaterialData);
        }


        public void SetupMaterial(Entity entity, SunBurstMaterialData sunBurstMaterialData)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var manager = world.EntityManager;
            // 1. Создаем уникальный экземпляр материала для этого спрайта
            // Примечание: Если спрайтов много, лучше использовать Property Blocks, 
            // но для начала создадим экземпляр:
            Material instanceMat = new Material(_sunBurstMaterialTemplate);
            instanceMat.SetColor("_BaseColor", sunBurstMaterialData.Color1);
            instanceMat.SetColor("_LineColor", sunBurstMaterialData.Color2);
            instanceMat.SetFloat("_LineCount", sunBurstMaterialData.LineCount);
            instanceMat.SetFloat("_Twist", sunBurstMaterialData.TwistFactor);
            instanceMat.mainTexture = _quadSprite.texture;

            // 2. Описываем массив мешей и материалов
            var renderMeshArray = new RenderMeshArray(new[] { instanceMat }, new[] { _quadMesh });
        
            var renderMeshDescription = new RenderMeshDescription
            {
                FilterSettings = RenderFilterSettings.Default,
                LightProbeUsage = LightProbeUsage.Off
            };

            // 3. Добавляем компоненты рендера на существующую сущность
            RenderMeshUtility.AddComponents(
                entity, 
                manager, 
                renderMeshDescription, 
                renderMeshArray, 
                MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0)
            );

            manager.AddComponentData(entity, LocalTransform.FromPositionRotationScale(
                float3.zero, 
                quaternion.identity, 
                1.0f // Базовый масштаб
            ));
        }
        
        public void Remove(Entity entity)
        {
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
                typeof(SunBurstMaterialData)
            );

            entityManager.RemoveComponent(entity, typesToRemove);
        }
    }
}