using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

public class AddAnEntitySprite : MonoBehaviour
{
    public Material baseMaterial;
    public Mesh quadMesh;
    
    public void SetupSpriteRender(Entity entity, Sprite sprite)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var manager = world.EntityManager;
        // 1. Создаем уникальный экземпляр материала для этого спрайта
        // Примечание: Если спрайтов много, лучше использовать Property Blocks, 
        // но для начала создадим экземпляр:
        Material instanceMat = new Material(baseMaterial);

        instanceMat.mainTexture = sprite.texture;

        // 2. Описываем массив мешей и материалов
        var renderMeshArray = new RenderMeshArray(new[] { instanceMat }, new[] { quadMesh });
        
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
                
        // 4. Устанавливаем корректный масштаб на основе размеров спрайта (Pixels Per Unit)
        float3 spriteScale = new float3(
            sprite.rect.width / sprite.pixelsPerUnit,
            sprite.rect.height / sprite.pixelsPerUnit,
            1f
        );

        manager.AddComponentData(entity, LocalTransform.FromPositionRotationScale(
            float3.zero, 
            quaternion.identity, 
            1.0f // Базовый масштаб
        ));
        
        // Добавляем PostTransformMatrix, если нужно подогнать размер под спрайт без изменения LocalTransform
        manager.AddComponentData(entity, new PostTransformMatrix { Value = float4x4.Scale(spriteScale) });
    }
    
    public void SetupSpriteRender(Entity entity, Texture texture, Material material)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var manager = world.EntityManager;
        // 1. Создаем уникальный экземпляр материала для этого спрайта
        // Примечание: Если спрайтов много, лучше использовать Property Blocks, 
        // но для начала создадим экземпляр:
        Material instanceMat = material;
        instanceMat.mainTexture = texture;

        // 2. Описываем массив мешей и материалов
        var renderMeshArray = new RenderMeshArray(new[] { instanceMat }, new[] { quadMesh });
        
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
                
        // 4. Устанавливаем корректный масштаб на основе размеров спрайта (Pixels Per Unit)



        manager.AddComponentData(entity, LocalTransform.FromPositionRotationScale(
            float3.zero, 
            quaternion.identity, 
            1.0f // Базовый масштаб
        ));

    }
}