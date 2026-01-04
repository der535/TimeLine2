using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

class OutlineRenderPass : ScriptableRenderPass
{
    private const string k_OutlinePassName = "OutlineRenderPass";
    private static readonly int outlineColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int outlineWidthId = Shader.PropertyToID("_OutlineWidth");
    
    private Material outlineMaterial;
    private OutlineRenderFeature.OutlineSettings settings;

    public OutlineRenderPass(Material material, OutlineRenderFeature.OutlineSettings settings)
    {
        this.outlineMaterial = material;
        this.settings = settings;
        renderPassEvent = RenderPassEvent.AfterRenderingSkybox; // Ключевое изменение!
    }

    public void UpdateSettings(OutlineRenderFeature.OutlineSettings newSettings)
    {
        this.settings = newSettings;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        
        // ВРЕМЕННО УБИРАЕМ ВСЕ ПРОВЕРКИ ДЛЯ ОТЛАДКИ
        // if (resourceData.isActiveTargetBackBuffer) return;
        
        TextureHandle cameraColor = resourceData.activeColorTexture;
        if (!cameraColor.IsValid())
        {
            Debug.LogError("[Outline] Camera color texture is invalid!");
            return;
        }

        // Создаем тестовый объект для отладки
        if (outlineMaterial != null)
        {
            outlineMaterial.SetColor(outlineColorId, Color.red); // ЯРКО-КРАСНЫЙ
            outlineMaterial.SetFloat(outlineWidthId, 10f); // БОЛЬШАЯ ТОЛЩИНА
        }

        using (var builder = renderGraph.AddRasterRenderPass<OutlinePassData>(k_OutlinePassName, out var passData))
        {
            builder.SetRenderAttachment(cameraColor, 0, AccessFlags.Write);
            
            passData.outlineMaterial = outlineMaterial;
            
            builder.SetRenderFunc((OutlinePassData data, RasterGraphContext context) =>
            {
                // Рисуем ОДИН тестовый спрайт для отладки
                GameObject testObject = GameObject.Find("DebugOutlineTest");
                if (testObject == null)
                {
                    testObject = new GameObject("DebugOutlineTest");
                    SpriteRenderer sr = testObject.AddComponent<SpriteRenderer>();
                    sr.sprite = Resources.GetBuiltinResource<Sprite>("Default-Sprite.sprite");
                    sr.color = Color.clear; // Прозрачный оригинальный спрайт
                    sr.transform.position = Vector3.zero;
                    sr.transform.localScale = Vector3.one * 2f;
                }
                
                SpriteRenderer renderer = testObject.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    Debug.Log("[Outline] Drawing debug outline"); // Отладочный лог
                    context.cmd.DrawRenderer(renderer, data.outlineMaterial);
                }
            });
        }
    }

    private class OutlinePassData
    {
        public Material outlineMaterial;
    }
}